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
        /// The filters that have been cached. These can be filtered by <see cref="IBasicDataFilter.IsActive"/> to find the filters that are in active use.
        /// <para>
        ///     This is for reference only and should not be relied upon. Please use <see cref="IsActive{TFilter}"/> or <see cref="IsEnabled{TFilter}"/> for accurate results.
        /// </para>
        /// </summary>
        IReadOnlyDictionary<Type, IBasicDataFilter> ReadOnlyFilters { get; }

        /// <summary>
        /// <para>
        ///     Enables the given filter. This should be disposed of after use to reset the filter to its previous state!
        /// </para>
        /// <example>
        ///     <b>Example:</b> <c>using(DataFilter.Enable&lt;ISoftDelete&gt;) { /* code here */ }</c>
        /// </example>
        /// </summary>
        /// <typeparam name="TFilter">The filter to enable. </typeparam>
        /// <returns>A disposable which resets the filter after use. </returns>
        IDisposable Enable<TFilter>() where TFilter : class;

        /// <summary>
        /// <para>
        ///     Disables the given filter. This should be disposed of after use to reset the filter to its previous state!
        /// </para>
        /// <example>
        ///     <b>Example:</b> <c>using(DataFilter.Disable&lt;ISoftDelete&gt;) { /* code here */ }</c>
        /// </example>
        /// </summary>
        /// <typeparam name="TFilter">The filter to disable. </typeparam>
        /// <returns>A disposable which resets the filter after use. </returns>
        IDisposable Disable<TFilter>() where TFilter : class;

        /// <summary>
        /// <para>
        ///     Determines if the <see cref="IDataFilter{TFilter}"/> is active for the given <typeparamref name="TFilter"/>.
        ///     <br/>A filter is active when <see cref="Enable{TFilter}"/> or <see cref="Disable{TFilter}"/> has been called and is in active use.
        ///     <br/>When the filter is disposed, it will be set to inactive again.
        /// </para>
        /// <example>
        ///     Example usage: <see cref="DataFilter.IsActive{TFilter}"/>
        /// </example>
        /// </summary>
        /// <typeparam name="TFilter">The <see cref="Type"/> of filter to find i.e. <see cref="ISoftDelete"/>. </typeparam>
        /// <returns>
        ///     <see langword="true"/> if the filter is active for the given <typeparamref name="TFilter"/>, 
        ///     otherwise <see langword="false"/>.
        /// </returns>
        bool IsActive<TFilter>() where TFilter : class;

        /// <summary>
        /// <para>
        ///     Determines if the <see cref="IDataFilter{TFilter}"/> is active for the given <paramref name="filterType"/>.
        ///     <br/>A filter is active when <see cref="Enable{TFilter}"/> or <see cref="Disable{TFilter}"/> has been called and is in active use.
        ///     <br/>When the filter is disposed, it will be set to inactive again.
        /// </para>
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

        /// <summary>
        /// <para>
        ///     Determines if the <see cref="IDataFilter{TFilter}"/> is enabled for the given <typeparamref name="TFilter"/>.
        /// </para>
        /// <example>
        ///     Example usage: <see cref="DataFilter.IsEnabled{TFilter}"/>
        /// </example>
        /// </summary>
        /// <typeparam name="TFilter">The <see cref="Type"/> of filter to find i.e. <see cref="ISoftDelete"/>. </typeparam>
        /// <returns>
        ///     <see langword="true"/> if the filter is enabled for the given <paramref name="filterType"/>, 
        ///     otherwise the default value for this filter defined in the <see cref="AbpDataFilterOptions.DefaultStates"/> (normally <see langword="true"/>). 
        /// </returns>
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
        ///     otherwise the default value for this filter defined in the <see cref="AbpDataFilterOptions.DefaultStates"/> (normally <see langword="true"/>). 
        /// </returns>
        bool IsEnabled(Type filterType);

        /// <summary>
        /// Gets the <see cref="IDataFilter{TFilter}"/> representing the <typeparamref name="TFilter"/>.
        /// If the filter doesn't exist then a new instance will be created.
        /// </summary>
        /// <typeparam name="TFilter">The type of filter e.g. <see cref="ISoftDelete"/>. </typeparam>
        IDataFilter<TFilter> GetOrAddFilter<TFilter>() where TFilter : class;

        /// <summary>
        /// Gets the <see cref="IBasicDataFilter"/> representing the <paramref name="filterType"/>.
        /// If the filter doesn't exist then a new instance will be created.
        /// <para>
        ///     <b>Note</b>: This method should only be used when the <see cref="IDataFilter{TFilter}"/> type is not known until runtime as there is a performance penalty for using it.
        ///     <br/>Please use <see cref="GetOrAddFilter{TFilter}"/> if the filter type is known at compile-time e.g. when using the generic <see cref="ISoftDelete"/>.
        /// </para>
        /// <para>
        ///     It should be possible to cast the <see cref="IBasicDataFilter"/> to your dynamic type.
        ///     <br/>Example: <c>(myDynamicFilter as IDataFilter&lt;ISoftDelete&lt;MyClass&gt;&gt;)</c>
        /// </para>
        /// </summary>
        /// <param name="filterType">The type of filter e.g. <c>typeof(myDynamicFilter)</c>. </param>
        IBasicDataFilter GetOrAddFilter(Type filterType);
    }
}
