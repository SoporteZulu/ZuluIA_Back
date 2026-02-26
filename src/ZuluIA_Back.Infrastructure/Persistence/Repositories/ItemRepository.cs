using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Items;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Infrastructure.Persistence.Repositories;

public class ItemRepository(AppDbContext context)
    : BaseRepository<Item>(context), IItemRepository
{
    public async Task<Item?> GetByCodigoAsync(
        string codigo,
        long? sucursalId,
        CancellationToken ct = default)
    {
        var query = DbSet.Where(x => x.Codigo == codigo.ToUpperInvariant());

        if (sucursalId.HasValue)
            query = query.Where(x => x.SucursalId == sucursalId.Value);

        return await query.FirstOrDefaultAsync(ct);
    }

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

    public async Task<bool> ExisteCodigoAsync(
        string codigo,
        long? sucursalId,
        long? excludeId = null,
        CancellationToken ct = default)
    {
        var query = DbSet.Where(x => x.Codigo == codigo.ToUpperInvariant());

        if (sucursalId.HasValue)
            query = query.Where(x => x.SucursalId == sucursalId.Value);

        if (excludeId.HasValue)
            query = query.Where(x => x.Id != excludeId.Value);

        return await query.AnyAsync(ct);
    }
}