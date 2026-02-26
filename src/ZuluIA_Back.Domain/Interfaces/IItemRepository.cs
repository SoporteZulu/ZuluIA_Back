using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Items;

namespace ZuluIA_Back.Domain.Interfaces;

public interface IItemRepository : IRepository<Item>
{
    Task<Item?> GetByCodigoAsync(string codigo, long? sucursalId, CancellationToken ct = default);
    Task<PagedResult<Item>> GetPagedAsync(int page, int pageSize, string? search, bool? soloProductos, bool? soloServicios, CancellationToken ct = default);
    Task<bool> ExisteCodigoAsync(string codigo, long? sucursalId, long? excludeId = null, CancellationToken ct = default);
}