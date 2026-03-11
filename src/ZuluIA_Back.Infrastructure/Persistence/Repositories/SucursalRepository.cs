using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Domain.Entities.Sucursales;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Infrastructure.Persistence.Repositories;

public class SucursalRepository(AppDbContext context)
    : BaseRepository<Sucursal>(context), ISucursalRepository
{
    public async Task<IReadOnlyList<Sucursal>> GetAllActivasAsync(CancellationToken ct = default) =>
        await DbSet
            .AsNoTracking()
            .Where(x => x.Activa)
            .OrderBy(x => x.RazonSocial)
            .ToListAsync(ct);

    public async Task<bool> ExisteCuitAsync(
        string cuit,
        long? excludeId = null,
        CancellationToken ct = default)
    {
        var query = DbSet.Where(x => x.Cuit == cuit.Trim());
        if (excludeId.HasValue)
            query = query.Where(x => x.Id != excludeId.Value);
        return await query.AnyAsync(ct);
    }

    public async Task<Sucursal?> GetCasaMatrizAsync(CancellationToken ct = default) =>
        await DbSet.FirstOrDefaultAsync(x => x.CasaMatriz && x.Activa, ct);
}