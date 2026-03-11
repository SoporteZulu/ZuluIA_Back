using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Finanzas;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Infrastructure.Persistence.Repositories;

public class MovimientoCtaCteRepository(AppDbContext context)
    : BaseRepository<MovimientoCtaCte>(context), IMovimientoCtaCteRepository
{
    public async Task<PagedResult<MovimientoCtaCte>> GetPagedAsync(
        int page, int pageSize,
        long terceroId, long? sucursalId, long? monedaId,
        DateOnly? desde, DateOnly? hasta,
        CancellationToken ct = default)
    {
        var query = DbSet.AsNoTracking()
            .Where(x => x.TerceroId == terceroId);

        if (sucursalId.HasValue) query = query.Where(x => x.SucursalId == sucursalId.Value);
        if (monedaId.HasValue) query = query.Where(x => x.MonedaId   == monedaId.Value);
        if (desde.HasValue) query = query.Where(x => x.Fecha >= desde.Value);
        if (hasta.HasValue) query = query.Where(x => x.Fecha <= hasta.Value);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(x => x.Fecha)
            .ThenByDescending(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<MovimientoCtaCte>(items, page, pageSize, total);
    }
}