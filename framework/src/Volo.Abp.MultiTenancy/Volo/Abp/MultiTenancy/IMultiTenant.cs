using System;
using Volo.Abp.Data;

namespace Volo.Abp.MultiTenancy
{
    /// <summary>
    /// Used to filter entities by <see cref="TenantId"/>.
    /// </summary>
    /// <para>
    ///     Please use the <see cref="IMultiTenant{TEntity}"/> interface to mark your entities 
    ///     as this interface inherits from <see cref="IMultiTenant"/> anyway.
    /// </para>
    /// <remarks>
    ///     Example filter usage: <see cref="DataFilter.Disable{IMultiTenant}()"/>
    /// </remarks>
    public interface IMultiTenant
    {
        /// <summary>
        /// Id of the related tenant.
        /// </summary>
        Guid? TenantId { get; }
    }

#pragma warning disable CS1584 // XML comment has syntactically incorrect cref attribute
#pragma warning disable CS1658 // Warning is overriding an error
    /// <summary>
    /// Used to filter entities by <see cref="IMultiTenant.TenantId"/>.
    /// <para>
    ///     This is used as a marker to enable/disable <see cref="IMultiTenant"/> filters specifically for the 
    ///     entitiy specified by <typeparamref name="TEntity"></typeparamref>.
    /// </para>
    /// <para>
    ///     <b>NOTE:</b> Please use this interface to mark your entities instead of the generic <see cref="IMultiTenant"/>
    /// </para>
    /// </summary>
    /// <remarks>
    ///     Example filter usage: <see cref="DataFilter.Disable{IMultiTenant{TEntity}}()"/>
    /// </remarks>
    public interface IMultiTenant<TEntity> : IMultiTenant where TEntity : class { }
#pragma warning restore CS1658 // Warning is overriding an error
#pragma warning restore CS1584 // XML comment has syntactically incorrect cref attribute
}
