using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Finanzas;
using ZuluIA_Back.Domain.Enums;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Infrastructure.Persistence.Repositories;

public class CedulonRepository(AppDbContext context)
    : BaseRepository<Cedulon>(context), ICedulonRepository
{
    public async Task<PagedResult<Cedulon>> GetPagedAsync(
        int page, int pageSize,
        long? terceroId, long? sucursalId, EstadoCedulon? estado,
        CancellationToken ct = default)
    {
        var query = DbSet.AsNoTracking();

        if (terceroId.HasValue) query = query.Where(x => x.TerceroId  == terceroId.Value);
        if (sucursalId.HasValue) query = query.Where(x => x.SucursalId == sucursalId.Value);
        if (estado.HasValue) query = query.Where(x => x.Estado     == estado.Value);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(x => x.FechaVencimiento)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<Cedulon>(items, page, pageSize, total);
    }

    public async Task<Cedulon?> GetByNroAsync(
        string nroCedulon,
        CancellationToken ct = default) =>
        await DbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.NroCedulon == nroCedulon.Trim(), ct);
}