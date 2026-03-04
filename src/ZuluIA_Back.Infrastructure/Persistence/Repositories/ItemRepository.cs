using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Items;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Infrastructure.Persistence.Repositories;

public class ItemRepository(AppDbContext context)
    : BaseRepository<Item>(context), IItemRepository
{
    // Paginado con filtros avanzados
    public async Task<PagedResult<Item>> GetPagedAsync(
        int page,
        int pageSize,
        string? search,
        long? categoriaId,
        bool? soloActivos,
        bool? soloConStock,
        CancellationToken ct = default)
    {
        var query = DbSet.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLower();
            query = query.Where(x =>
                x.Codigo.ToLower().Contains(term)          ||
                x.Descripcion.ToLower().Contains(term)     ||
                (x.CodigoBarras != null &&
                 x.CodigoBarras.ToLower().Contains(term)));
        }

        if (categoriaId.HasValue)
            query = query.Where(x => x.CategoriaId == categoriaId.Value);

        if (soloActivos.HasValue)
            query = query.Where(x => x.Activo == soloActivos.Value);

        if (soloConStock == true)
            query = query.Where(x => x.ManejaStock && x.StockMinimo >= 0);

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderBy(x => x.Codigo)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<Item>(items, page, pageSize, total);
    }

    // Paginado con filtros básicos (productos/servicios y sucursal)
    public async Task<PagedResult<Item>> GetPagedAsync(
        int page,
        int pageSize,
        string? search,
        bool? soloProductos,
        bool? soloServicios,
        CancellationToken ct = default)
    {
        var query = DbSet.AsNoTracking().Where(x => x.Activo);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLower();
            query = query.Where(x =>
                x.Descripcion.ToLower().Contains(term)  ||
                x.Codigo.ToLower().Contains(term)       ||
                (x.CodigoBarras != null && x.CodigoBarras.ToLower().Contains(term)));
        }

        if (soloProductos == true)
            query = query.Where(x => x.EsProducto);

        if (soloServicios == true)
            query = query.Where(x => x.EsServicio);

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderBy(x => x.Descripcion)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<Item>(items, page, pageSize, total);
    }

    // Búsqueda por código y sucursal
    public async Task<Item?> GetByCodigoAsync(
        string codigo,
        long? sucursalId,
        CancellationToken ct = default)
    {
        var query = DbSet.Where(x => x.Codigo == codigo.Trim().ToUpperInvariant());

        if (sucursalId.HasValue)
            query = query.Where(x => x.SucursalId == sucursalId.Value);

        return await query.FirstOrDefaultAsync(ct);
    }

    // Búsqueda por código (sin sucursal)
    public async Task<Item?> GetByCodigoAsync(
        string codigo,
        CancellationToken ct = default) =>
        await DbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.Codigo == codigo.Trim().ToUpperInvariant(), ct);

    // Búsqueda por código de barras
    public async Task<Item?> GetByCodigoBarrasAsync(
        string codigoBarras,
        CancellationToken ct = default) =>
        await DbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.CodigoBarras == codigoBarras.Trim(), ct);

    // Verifica existencia por código y sucursal (para edición)
    public async Task<bool> ExisteCodigoAsync(
        string codigo,
        long? sucursalId,
        long? excludeId = null,
        CancellationToken ct = default)
    {
        var query = DbSet.Where(x => x.Codigo == codigo.Trim().ToUpperInvariant());

        if (sucursalId.HasValue)
            query = query.Where(x => x.SucursalId == sucursalId.Value);

        if (excludeId.HasValue)
            query = query.Where(x => x.Id != excludeId.Value);

        return await query.AnyAsync(ct);
    }

    // Verifica existencia por código (sin sucursal)
    public async Task<bool> ExisteCodigoAsync(
        string codigo,
        long? excludeId = null,
        CancellationToken ct = default)
    {
        var query = DbSet.Where(x =>
            x.Codigo == codigo.Trim().ToUpperInvariant());

        if (excludeId.HasValue)
            query = query.Where(x => x.Id != excludeId.Value);

        return await query.AnyAsync(ct);
    }

    // Listado por categoría
    public async Task<IReadOnlyList<Item>> GetByCategoria(
        long categoriaId,
        CancellationToken ct = default) =>
        await DbSet
            .AsNoTracking()
            .Where(x => x.CategoriaId == categoriaId && x.Activo)
            .OrderBy(x => x.Codigo)
            .ToListAsync(ct);
}
