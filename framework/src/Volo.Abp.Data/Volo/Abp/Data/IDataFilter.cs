using System;
using System.Collections.Generic;

namespace Volo.Abp.Data
{
    public interface IDataFilter<TFilter>
        where TFilter : class
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

        IDisposable Enable<TFilter>()
            where TFilter : class;

        IDisposable Disable<TFilter>()
            where TFilter : class;

        bool IsEnabled<TFilter>()
            where TFilter : class;

        /// <summary>
        /// Used when you don't know the type at compile-time. 
        /// <para>
        ///     NOTE: There is a performance penalty for using this method due to the use of Reflection. 
        ///     Please use <see cref="IsEnabled{TFilter}"/> is possible.
        /// </para>
        /// </summary>
        /// <param name="filterType">The filter to find e.g. <see cref="ISoftDelete{TEntity}"/></param>
        /// <returns>
        ///     <see langword="true"/> if the filter is found, 
        ///     otherwise false if the <paramref name="filterType"/> is invalid <see cref="Type"/> or 
        ///     it is not enabled.
        /// </returns>
        bool IsEnabled(Type filterType);
    }
}
