using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.RRHH;
using ZuluIA_Back.Domain.Enums;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Infrastructure.Persistence.Repositories;

public class EmpleadoRepository(AppDbContext context)
    : BaseRepository<Empleado>(context), IEmpleadoRepository
{
    public async Task<PagedResult<Empleado>> GetPagedAsync(
        int page, int pageSize,
        long? sucursalId, EstadoEmpleado? estado, string? search,
        CancellationToken ct = default)
    {
        var query = DbSet.AsNoTracking();

        if (sucursalId.HasValue) query = query.Where(x => x.SucursalId == sucursalId.Value);
        if (estado.HasValue) query = query.Where(x => x.Estado     == estado.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLower();
            query = query.Where(x =>
                x.Legajo.ToLower().Contains(term) ||
                x.Cargo.ToLower().Contains(term)  ||
                (x.Area != null && x.Area.ToLower().Contains(term)));
        }

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(x => x.Legajo)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<Empleado>(items, page, pageSize, total);
    }

    public async Task<Empleado?> GetByLegajoAsync(
        string legajo,
        CancellationToken ct = default) =>
        await DbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Legajo == legajo.Trim().ToUpperInvariant(), ct);

    public async Task<bool> ExisteLegajoAsync(
        string legajo,
        long? excludeId,
        CancellationToken ct = default)
    {
        var query = DbSet.Where(x =>
            x.Legajo == legajo.Trim().ToUpperInvariant());

        if (excludeId.HasValue)
            query = query.Where(x => x.Id != excludeId.Value);

        return await query.AnyAsync(ct);
    }
}