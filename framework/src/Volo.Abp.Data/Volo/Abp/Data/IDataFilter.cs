using System;
using System.Collections.Generic;

namespace Volo.Abp.Data
{
    public interface IDataFilter<TFilter> : IToggleDataFilter
        where TFilter : class { }

    public interface IToggleDataFilter
    {
        IDisposable Enable();

        IDisposable Disable();

        bool IsEnabled { get; }
    }

    public interface IDataFilter
    {
        /// <summary>
        /// The filters that are currently active.
        /// </summary>
        IReadOnlyDictionary<Type, object> ReadOnlyFilters { get; }
        /// <summary>
        /// The current <see cref="AbpDataFilterOptions.DefaultStates"/>.
        /// </summary>
        IReadOnlyDictionary<Type, DataFilterState> DefaultFilterStates { get; }

        IDisposable Enable<TFilter>()
            where TFilter : class;

        IDisposable Disable<TFilter>()
            where TFilter : class;

        bool IsEnabled<TFilter>(bool cacheResult = true)
            where TFilter : class;

        /// <summary>
        /// Determines if the <see cref="IDataFilter{TFilter}"/> is enabled for the given <paramref name="filterType"/>.
        /// <para>
        ///     <b>Note</b>: This method should only be used when the <see cref="IDataFilter{TFilter}"/> type is not known until runtime as there is a performance penalty for using it.
        ///     <br/>Please use <see cref="IsEnabled{TFilter}"/> if the filter type is known at compile-time e.g. when using the generic <see cref="ISoftDelete"/>.
        /// </para>
        /// <example>
        ///     Example usage: <c>DataFilter.IsEnabled(typeof(ISoftDelete&lt;Blog&gt;))</c>
        ///     <br/><b>Don't use</b> when: <c>DataFilter.IsEnabled(typeof(ISoftDelete))</c>
        /// </example>
        /// </summary>
        /// <param name="filterType">The <see cref="Type"/> of filter to find i.e. <see cref="ISoftDelete{TEntity}"/>. </param>
        /// <param name="cacheResult">Should the value be cached in the <see cref="ReadOnlyFilters"/> collection? </param>
        /// <returns>
        ///     <see langword="true"/> if the filter is enabled for the given <paramref name="filterType"/>, 
        ///     otherwise the default value for this filter defined in the <see cref="DefaultFilterStates"/> (normally <see langword="true"/>). 
        /// </returns>
        bool IsEnabled(Type filterType, bool cacheResult = true);
    }
}
