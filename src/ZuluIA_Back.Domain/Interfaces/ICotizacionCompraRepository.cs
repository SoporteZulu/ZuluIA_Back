using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Compras;

namespace ZuluIA_Back.Domain.Interfaces;

public interface ICotizacionCompraRepository : IRepository<CotizacionCompra>
{
    Task<PagedResult<CotizacionCompra>> GetPagedAsync(
        int page,
        int pageSize,
        long? sucursalId,
        long? proveedorId,
        string? estado,
        CancellationToken ct = default);

    Task<CotizacionCompra?> GetByIdConItemsAsync(long id, CancellationToken ct = default);
}
