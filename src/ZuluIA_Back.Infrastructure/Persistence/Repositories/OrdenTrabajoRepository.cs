using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Produccion;
using ZuluIA_Back.Domain.Enums;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Infrastructure.Persistence.Repositories;

public class OrdenTrabajoRepository(AppDbContext context)
    : BaseRepository<OrdenTrabajo>(context), IOrdenTrabajoRepository
{
    public async Task<PagedResult<OrdenTrabajo>> GetPagedAsync(
        int page, int pageSize,
        long? sucursalId, long? formulaId,
        EstadoOrdenTrabajo? estado,
        DateOnly? desde, DateOnly? hasta,
        CancellationToken ct = default)
    {
        var query = DbSet.AsNoTracking();

        if (sucursalId.HasValue) query = query.Where(x => x.SucursalId == sucursalId.Value);
        if (formulaId.HasValue) query = query.Where(x => x.FormulaId  == formulaId.Value);
        if (estado.HasValue) query = query.Where(x => x.Estado     == estado.Value);
        if (desde.HasValue) query = query.Where(x => x.Fecha      >= desde.Value);
        if (hasta.HasValue) query = query.Where(x => x.Fecha      <= hasta.Value);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(x => x.Fecha)
            .ThenByDescending(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<OrdenTrabajo>(items, page, pageSize, total);
    }
}