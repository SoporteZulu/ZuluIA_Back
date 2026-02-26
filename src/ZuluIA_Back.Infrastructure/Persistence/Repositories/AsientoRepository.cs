using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Contabilidad;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Infrastructure.Persistence.Repositories;

public class AsientoRepository(AppDbContext context)
    : BaseRepository<Asiento>(context), IAsientoRepository
{
    public async Task<long> GetProximoNumeroAsync(
        long ejercicioId,
        long sucursalId,
        CancellationToken ct = default)
    {
        var ultimo = await DbSet
            .Where(x => x.EjercicioId == ejercicioId && x.SucursalId == sucursalId)
            .MaxAsync(x => (long?)x.Numero, ct);

        return (ultimo ?? 0) + 1;
    }

    public async Task<PagedResult<Asiento>> GetPagedAsync(
        int page,
        int pageSize,
        long ejercicioId,
        long? sucursalId,
        string? estado,
        CancellationToken ct = default)
    {
        var query = DbSet
            .AsNoTracking()
            .Include(x => x.Lineas)
            .Where(x => x.EjercicioId == ejercicioId);

        if (sucursalId.HasValue)
            query = query.Where(x => x.SucursalId == sucursalId.Value);

        if (!string.IsNullOrWhiteSpace(estado))
            query = query.Where(x => x.Estado.ToString() == estado);

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(x => x.Fecha)
            .ThenByDescending(x => x.Numero)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<Asiento>(items, page, pageSize, total);
    }

    public override async Task<Asiento?> GetByIdAsync(long id, CancellationToken ct = default) =>
        await DbSet
            .Include(x => x.Lineas)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
}