using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Finanzas;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Infrastructure.Persistence.Repositories;

public class PagoRepository(AppDbContext context)
    : BaseRepository<Pago>(context), IPagoRepository
{
    public async Task<PagedResult<Pago>> GetPagedAsync(
        int page, int pageSize,
        long? sucursalId, long? terceroId,
        DateOnly? desde, DateOnly? hasta,
        CancellationToken ct = default)
    {
        var query = DbSet.AsNoTracking();

        if (sucursalId.HasValue) query = query.Where(x => x.SucursalId == sucursalId.Value);
        if (terceroId.HasValue) query = query.Where(x => x.TerceroId  == terceroId.Value);
        if (desde.HasValue) query = query.Where(x => x.Fecha >= desde.Value);
        if (hasta.HasValue) query = query.Where(x => x.Fecha <= hasta.Value);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(x => x.Fecha)
            .ThenByDescending(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<Pago>(items, page, pageSize, total);
    }

    public async Task<Pago?> GetByIdConMediosAsync(
        long id,
        CancellationToken ct = default) =>
        await DbSet
            .Include(x => x.Medios)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
}