using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Volo.Abp.DependencyInjection;

namespace Volo.Abp.Data
{
    //TODO: Create a Volo.Abp.Data.Filtering namespace?
    public class DataFilter : IDataFilter, ISingletonDependency
    {
        public IReadOnlyDictionary<Type, object> ReadOnlyFilters => Filters;

        public IReadOnlyDictionary<Type, DataFilterState> DefaultFilterStates  => FilterOptions.DefaultStates;

        protected readonly ConcurrentDictionary<Type, object> Filters;

        protected readonly AbpDataFilterOptions FilterOptions;

        protected readonly IServiceProvider ServiceProvider;

        public DataFilter(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
            Filters = new ConcurrentDictionary<Type, object>();
            FilterOptions = ServiceProvider
                .GetRequiredService<IOptions<AbpDataFilterOptions>>()
                .Value;
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

        public virtual bool IsEnabled<TFilter>(bool cacheResult = true)
            where TFilter : class
        {
            return GetFilter<TFilter>(cacheResult).IsEnabled;
        }

        public virtual bool IsEnabled(Type filterType, bool cacheResult = true)
        {
            if (filterType == null
                // Should have no more than 1 interface type argument
                || filterType.GenericTypeArguments.Length > 1
                // Should be a generic filter interface e.g. ISoftDelete
                || (filterType.GenericTypeArguments.Length == 0 && !filterType.IsInterface)
                // Should be a filter interface with a concrete parameter e.g. Blog (filter == ISoftDelete<Blog>)
                || (filterType.GenericTypeArguments.Length == 1 && filterType.GenericTypeArguments[0].IsGenericType))
            {
                throw new AbpException($"The {nameof(filterType)} '{(filterType == null ? "<null>" : filterType.Name)}' is not a valid type for {nameof(IsEnabled)}");
            }

            var foundFilter = GetFilter(filterType, cacheResult);

            if (foundFilter != null)
            {
                // todo: Can we avoid using magic strings here for "IsEnabled"?
                return (bool)foundFilter.GetType()
                    .GetProperty("IsEnabled", BindingFlags.Public | BindingFlags.Instance)
                    .GetValue(foundFilter);
            }

            return DefaultFilterStates.GetOrDefault(filterType)?.IsEnabled ?? FilterOptions.DefaultFilterState;
        }

        /// <summary>
        /// Gets the <see cref="IDataFilter{TFilter}"/> representing the <typeparamref name="TFilter"/>. 
        /// </summary>
        /// <typeparam name="TFilter">The type of filter e.g. <see cref="ISoftDelete"/>. </typeparam>
        /// <param name="cacheResult">Should the value be cached in the <see cref="ReadOnlyFilters"/> collection? </param>
        protected virtual IDataFilter<TFilter> GetFilter<TFilter>(bool cacheResult = true)
            where TFilter : class
        {
            if (cacheResult)
            {
                return Filters.GetOrAdd(
                    typeof(TFilter),
                    () => ServiceProvider.GetRequiredService<IDataFilter<TFilter>>()
                ) as IDataFilter<TFilter>;
            }
            else
            {
                // note: not using GetFilter(Type...) because this will be more performant

                if (Filters.TryGetValue(typeof(TFilter), out var value))
                {
                    return (IDataFilter<TFilter>)value;
                }

                return ServiceProvider.GetRequiredService<IDataFilter<TFilter>>();
            }
        }

        protected object GetFilter(Type filter, bool cacheResult = true)
        {
            if (cacheResult)
            {
                return Filters.GetOrAdd(
                    filter,
                    () => ServiceProvider.GetRequiredService(
                        typeof(IDataFilter<>).MakeGenericType(filter))
                );
            }
            else
            {
                if (Filters.TryGetValue(filter, out var value))
                {
                    return value;
                }

                return ServiceProvider.GetRequiredService(
                    typeof(IDataFilter<>).MakeGenericType(filter));
            }
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

            Filter.Value = Options.DefaultStates.GetOrDefault(typeof(TFilter))?.Clone() ?? new DataFilterState(Options.DefaultFilterState);
        }
    }
}