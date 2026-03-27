using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Agro;
using ZuluIA_Back.Domain.Interfaces;
using ZuluIA_Back.Infrastructure.Persistence;

namespace ZuluIA_Back.Infrastructure.Persistence.Repositories;

public class LiquidacionGranosRepository(AppDbContext context)
    : BaseRepository<LiquidacionGranos>(context), ILiquidacionGranosRepository
{
    public async Task<PagedResult<LiquidacionGranos>> GetPagedAsync(
        int page,
        int pageSize,
        long? sucursalId,
        long? terceroId,
        string? estado,
        CancellationToken ct = default)
    {
        var query = DbSet.AsNoTracking().Where(l => l.DeletedAt == null);

        if (sucursalId.HasValue) query = query.Where(l => l.SucursalId == sucursalId);
        if (terceroId.HasValue)  query = query.Where(l => l.TerceroId == terceroId);
        if (!string.IsNullOrWhiteSpace(estado))
            query = query.Where(l => l.Estado.ToString() == estado);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(l => l.Fecha)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<LiquidacionGranos>(items, page, pageSize, total);
    }

    public async Task<LiquidacionGranos?> GetByIdConConceptosAsync(long id, CancellationToken ct = default)
        => await DbSet.Include(l => l.Conceptos)
            .FirstOrDefaultAsync(l => l.Id == id, ct);
}
