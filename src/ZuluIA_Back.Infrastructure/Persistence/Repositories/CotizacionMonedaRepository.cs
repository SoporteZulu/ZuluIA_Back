using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Domain.Entities.Finanzas;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Infrastructure.Persistence.Repositories;

public class CotizacionMonedaRepository(AppDbContext context)
    : BaseRepository<CotizacionMoneda>(context), ICotizacionMonedaRepository
{
    public async Task<CotizacionMoneda?> GetVigenteAsync(
        long monedaId,
        DateOnly fecha,
        CancellationToken ct = default) =>
        await DbSet
            .AsNoTracking()
            .Where(x => x.MonedaId == monedaId && x.Fecha <= fecha)
            .OrderByDescending(x => x.Fecha)
            .FirstOrDefaultAsync(ct);

    public async Task<IReadOnlyList<CotizacionMoneda>> GetHistoricoAsync(
        long monedaId,
        DateOnly? desde,
        DateOnly? hasta,
        CancellationToken ct = default)
    {
        var query = DbSet
            .AsNoTracking()
            .Where(x => x.MonedaId == monedaId);

        if (desde.HasValue)
            query = query.Where(x => x.Fecha >= desde.Value);

        if (hasta.HasValue)
            query = query.Where(x => x.Fecha <= hasta.Value);

        return await query
            .OrderByDescending(x => x.Fecha)
            .ToListAsync(ct);
    }

    public async Task<bool> ExisteParaFechaAsync(
        long monedaId,
        DateOnly fecha,
        CancellationToken ct = default) =>
        await DbSet.AnyAsync(
            x => x.MonedaId == monedaId && x.Fecha == fecha,
            ct);
}