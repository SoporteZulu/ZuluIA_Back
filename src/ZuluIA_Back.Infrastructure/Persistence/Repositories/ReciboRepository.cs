using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Finanzas;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Infrastructure.Persistence.Repositories;

public class ReciboRepository(AppDbContext context)
    : BaseRepository<Recibo>(context), IReciboRepository
{
    public async Task<PagedResult<Recibo>> GetPagedAsync(
        int page, int pageSize,
        long? sucursalId, long? terceroId,
        DateOnly? desde, DateOnly? hasta,
        CancellationToken ct = default)
    {
        var query = DbSet.AsNoTracking();

        if (sucursalId.HasValue) query = query.Where(x => x.SucursalId == sucursalId.Value);
        if (terceroId.HasValue)  query = query.Where(x => x.TerceroId  == terceroId.Value);
        if (desde.HasValue)      query = query.Where(x => x.Fecha      >= desde.Value);
        if (hasta.HasValue)      query = query.Where(x => x.Fecha      <= hasta.Value);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(x => x.Fecha)
            .ThenByDescending(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<Recibo>(items, page, pageSize, total);
    }

    public async Task<Recibo?> GetByIdConItemsAsync(long id, CancellationToken ct = default) =>
        await DbSet
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<int> GetUltimoNumeroAsync(long sucursalId, string serie, CancellationToken ct = default) =>
        await DbSet
            .Where(x => x.SucursalId == sucursalId && x.Serie == serie)
            .MaxAsync(x => (int?)x.Numero, ct) ?? 0;
}
