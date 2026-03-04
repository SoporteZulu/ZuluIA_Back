using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Items;

namespace ZuluIA_Back.Domain.Interfaces;

public interface IItemRepository : IRepository<Item>
{
    // Paginado con filtros avanzados
    Task<PagedResult<Item>> GetPagedAsync(
        int page,
        int pageSize,
        string? search,
        long? categoriaId,
        bool? soloActivos,
        bool? soloConStock,
        CancellationToken ct = default);

    // Paginado con filtros básicos (por productos/servicios y sucursal)
    Task<PagedResult<Item>> GetPagedAsync(
        int page,
        int pageSize,
        string? search,
        bool? soloProductos,
        bool? soloServicios,
        CancellationToken ct = default);

    // Búsqueda por código y sucursal
    Task<Item?> GetByCodigoAsync(
        string codigo,
        long? sucursalId,
        CancellationToken ct = default);

    // Búsqueda por código (sin sucursal)
    Task<Item?> GetByCodigoAsync(
        string codigo,
        CancellationToken ct = default);

    // Búsqueda por código de barras
    Task<Item?> GetByCodigoBarrasAsync(
        string codigoBarras,
        CancellationToken ct = default);

    // Verifica existencia por código y sucursal (para edición)
    Task<bool> ExisteCodigoAsync(
        string codigo,
        long? sucursalId,
        long? excludeId = null,
        CancellationToken ct = default);

    // Verifica existencia por código (sin sucursal)
    Task<bool> ExisteCodigoAsync(
        string codigo,
        long? excludeId = null,
        CancellationToken ct = default);

    // Listado por categoría
    Task<IReadOnlyList<Item>> GetByCategoria(
        long categoriaId,
        CancellationToken ct = default);
}
