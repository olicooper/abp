using System;
using System.Collections.Generic;
using Volo.Abp.DependencyInjection;

namespace Volo.Abp.Data
{
    public interface IDataFilter<TFilter> : IBasicDataFilter
        where TFilter : class { }

    public interface IBasicDataFilter
    {
        IDisposable Enable();
        IDisposable Disable();
        bool IsActive { get; }
        bool IsEnabled { get; }
    }

    public interface IDataFilter : ISingletonDependency
    {
        /// <summary>
        /// The filters that are currently active.
        /// </summary>
        IReadOnlyDictionary<Type, IBasicDataFilter> ReadOnlyFilters { get; }
        /// <summary>
        /// The current <see cref="AbpDataFilterOptions.DefaultStates"/>.
        /// </summary>
        IReadOnlyDictionary<Type, DataFilterState> DefaultFilterStates { get; }

        IDisposable Enable<TFilter>() where TFilter : class;

        IDisposable Disable<TFilter>() where TFilter : class;

        bool IsActive<TFilter>() where TFilter : class;

        /// <summary>
        /// Determines if the <see cref="IDataFilter{TFilter}"/> is active for the given <paramref name="filterType"/>.
        /// A filter is active when <see cref="Enable{TFilter}"/> or <see cref="Disable{TFilter}"/> has been called.
        /// <para>
        ///     <b>Note</b>: This method should only be used when the <see cref="IDataFilter{TFilter}"/> type is not known until runtime as there is a performance penalty for using it.
        ///     <br/>Please use <see cref="IsActive{TFilter}"/> if the filter type is known at compile-time e.g. when using the generic <see cref="ISoftDelete"/>.
        /// </para>
        /// <example>
        ///     Example usage: <c>DataFilter.IsActive(typeof(myDynamicFilter))</c>
        /// </example>
        /// </summary>
        /// <param name="filterType">The <see cref="Type"/> of filter to find i.e. <see cref="ISoftDelete{TEntity}"/>. </param>
        /// <returns>
        ///     <see langword="true"/> if the filter is active for the given <paramref name="filterType"/>, 
        ///     otherwise <see langword="false"/>.
        /// </returns>
        bool IsActive(Type filterType);

        bool IsEnabled<TFilter>() where TFilter : class;

        /// <summary>
        /// Determines if the <see cref="IDataFilter{TFilter}"/> is enabled for the given <paramref name="filterType"/>.
        /// <para>
        ///     <b>Note</b>: This method should only be used when the <see cref="IDataFilter{TFilter}"/> type is not known until runtime as there is a performance penalty for using it.
        ///     <br/>Please use <see cref="IsEnabled{TFilter}"/> if the filter type is known at compile-time e.g. when using the generic <see cref="ISoftDelete"/>.
        /// </para>
        /// <example>
        ///     Example usage: <c>DataFilter.IsEnabled(typeof(myDynamicFilter))</c>
        /// </example>
        /// </summary>
        /// <param name="filterType">The <see cref="Type"/> of filter to find i.e. <see cref="ISoftDelete{TEntity}"/>. </param>
        /// <returns>
        ///     <see langword="true"/> if the filter is enabled for the given <paramref name="filterType"/>, 
        ///     otherwise the default value for this filter defined in the <see cref="DefaultFilterStates"/> (normally <see langword="true"/>). 
        /// </returns>
        bool IsEnabled(Type filterType);

        IDataFilter<TFilter> GetFilter<TFilter>() where TFilter : class;

        IBasicDataFilter GetFilter(Type filterType);
    }
}
