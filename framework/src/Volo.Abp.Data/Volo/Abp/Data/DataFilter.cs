using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Volo.Abp.DependencyInjection;

namespace Volo.Abp.Data
{
    //TODO: Create a Volo.Abp.Data.Filtering namespace?
    public class DataFilter : IDataFilter, ISingletonDependency
    {
        public IReadOnlyDictionary<Type, object> ReadOnlyFilters { get => Filters;  }
        protected readonly ConcurrentDictionary<Type, object> Filters;

        protected readonly IServiceProvider ServiceProvider;

        public DataFilter(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
            Filters = new ConcurrentDictionary<Type, object>();
        }

        public virtual IDisposable Enable<TFilter>()
            where TFilter : class
        {
            return GetFilter<TFilter>().Enable();
        }

        public virtual IDisposable Disable<TFilter>()
            where TFilter : class
        {
            return GetFilter<TFilter>().Disable();
        }

        public virtual bool IsEnabled<TFilter>()
            where TFilter : class
        {
            return GetFilter<TFilter>().IsEnabled;
        }

        public virtual bool IsEnabled(Type filterType)
        {
            if (filterType == null
                // Should be a filter interface with a concrete parameter e.g. Blog (filterType == ISoftDelete<Blog>)
                || (filterType.GenericTypeArguments.Length == 1 && filterType.GenericTypeArguments[0].IsGenericType)
                // Otherwise it should be a conrete type e.g. ISoftDelete
                || filterType.GenericTypeArguments.Length != 0)
            {
                return false;
            }

            try
            {
                var genericType = typeof(IDataFilter<>).MakeGenericType(filterType);
                var filter = Filters.GetOrAdd(
                    genericType,
                    () => ServiceProvider.GetRequiredService(genericType)
                );

                // todo: Can we avoid using magic strings here for "IsEnabled"?
                return (bool)genericType.InvokeMember(
                    "IsEnabled",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance,
                    Type.DefaultBinder,
                    filter,
                    new object[] { }
                );
            } 
            catch
            {
                return false;
            }
        }

        protected virtual IDataFilter<TFilter> GetFilter<TFilter>()
            where TFilter : class
        {
            return Filters.GetOrAdd(
                typeof(TFilter),
                () => ServiceProvider.GetRequiredService<IDataFilter<TFilter>>()
            ) as IDataFilter<TFilter>;
        }
    }

    public class DataFilter<TFilter> : IDataFilter<TFilter>
        where TFilter : class
    {
        public virtual bool IsEnabled
        {
            get
            {
                EnsureInitialized();
                return Filter.Value.IsEnabled;
            }
        }

        protected readonly AbpDataFilterOptions Options;

        protected readonly AsyncLocal<DataFilterState> Filter;

        public DataFilter(IOptions<AbpDataFilterOptions> options)
        {
            Options = options.Value;
            Filter = new AsyncLocal<DataFilterState>();
        }

        public virtual IDisposable Enable()
        {
            if (IsEnabled)
            {
                return NullDisposable.Instance;
            }

            Filter.Value.IsEnabled = true;

            return new DisposeAction(() => Disable());
        }

        public virtual IDisposable Disable()
        {
            if (!IsEnabled)
            {
                return NullDisposable.Instance;
            }

            Filter.Value.IsEnabled = false;

            return new DisposeAction(() => Enable());
        }

        protected virtual void EnsureInitialized()
        {
            if (Filter.Value != null)
            {
                return;
            }

            Filter.Value = Options.DefaultStates.GetOrDefault(typeof(TFilter))?.Clone() ?? new DataFilterState(true);
        }
    }
}