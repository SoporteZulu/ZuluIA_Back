using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Domain.Entities.Facturacion;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Infrastructure.Persistence.Repositories;

public class PeriodoIvaRepository(AppDbContext context)
    : BaseRepository<PeriodoIva>(context), IPeriodoIvaRepository
{
    public async Task<IReadOnlyList<PeriodoIva>> GetBySucursalAsync(
        long sucursalId,
        long? ejercicioId,
        CancellationToken ct = default)
    {
        var query = DbSet
            .AsNoTracking()
            .Where(x => x.SucursalId == sucursalId);

        if (ejercicioId.HasValue)
            query = query.Where(x => x.EjercicioId == ejercicioId.Value);

        return await query
            .OrderByDescending(x => x.Periodo)
            .ToListAsync(ct);
    }

    public async Task<PeriodoIva?> GetPeriodoAsync(
        long sucursalId,
        DateOnly periodo,
        CancellationToken ct = default)
    {
        var primerDia = new DateOnly(periodo.Year, periodo.Month, 1);
        return await DbSet
            .FirstOrDefaultAsync(x =>
                x.SucursalId == sucursalId &&
                x.Periodo    == primerDia,
                ct);
    }

    public async Task<bool> EstaAbiertoPeriodoAsync(
        long sucursalId,
        DateOnly fecha,
        CancellationToken ct = default)
    {
        var primerDia = new DateOnly(fecha.Year, fecha.Month, 1);
        var periodo = await DbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.SucursalId == sucursalId &&
                x.Periodo    == primerDia,
                ct);

        // Si no existe el período, se considera abierto (no se ha creado aún)
        return periodo is null || !periodo.Cerrado;
    }
}