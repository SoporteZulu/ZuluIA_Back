using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Domain.Entities.Finanzas;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Infrastructure.Persistence.Repositories;

public class CuentaCorrienteRepository(AppDbContext context)
    : BaseRepository<CuentaCorriente>(context), ICuentaCorrienteRepository
{
    public async Task<CuentaCorriente?> GetByTerceroMonedaAsync(
        long terceroId, long monedaId, long? sucursalId,
        CancellationToken ct = default) =>
        await DbSet.FirstOrDefaultAsync(x =>
            x.TerceroId  == terceroId  &&
            x.MonedaId   == monedaId   &&
            x.SucursalId == sucursalId, ct);

    public async Task<CuentaCorriente> GetOrCreateAsync(
        long terceroId, long monedaId, long? sucursalId,
        CancellationToken ct = default)
    {
        var cta = await DbSet.FirstOrDefaultAsync(x =>
            x.TerceroId  == terceroId  &&
            x.MonedaId   == monedaId   &&
            x.SucursalId == sucursalId, ct);

        if (cta is not null) return cta;

        cta = CuentaCorriente.Crear(terceroId, sucursalId, monedaId);
        await DbSet.AddAsync(cta, ct);
        return cta;
    }

    public async Task<IReadOnlyList<CuentaCorriente>> GetByTerceroAsync(
        long terceroId,
        CancellationToken ct = default) =>
        await DbSet
            .AsNoTracking()
            .Where(x => x.TerceroId == terceroId)
            .OrderBy(x => x.MonedaId)
            .ToListAsync(ct);
}