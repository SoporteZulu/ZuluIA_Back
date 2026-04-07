using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Compras;

namespace ZuluIA_Back.Domain.Interfaces;

public interface IRequisicionCompraRepository : IRepository<RequisicionCompra>
{
    Task<PagedResult<RequisicionCompra>> GetPagedAsync(
        int page,
        int pageSize,
        long? sucursalId,
        long? solicitanteId,
        string? estado,
        CancellationToken ct = default);

    Task<RequisicionCompra?> GetByIdConItemsAsync(long id, CancellationToken ct = default);
}
