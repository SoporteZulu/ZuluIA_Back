using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Ventas;

namespace ZuluIA_Back.Domain.Interfaces;

public interface INotaPedidoRepository : IRepository<NotaPedido>
{
    Task<PagedResult<NotaPedido>> GetPagedAsync(
        int page,
        int pageSize,
        long? sucursalId,
        long? terceroId,
        string? estado,
        CancellationToken ct = default);

    Task<NotaPedido?> GetByIdConItemsAsync(long id, CancellationToken ct = default);
    Task<IReadOnlyList<NotaPedido>> GetPendientesAsync(long sucursalId, CancellationToken ct = default);
}
