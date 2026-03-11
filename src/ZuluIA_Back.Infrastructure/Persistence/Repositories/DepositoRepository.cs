using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Domain.Entities.Items;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Infrastructure.Persistence.Repositories;

public class DepositoRepository(AppDbContext context)
    : BaseRepository<Deposito>(context), IDepositoRepository
{
    public async Task<IReadOnlyList<Deposito>> GetActivosBySucursalAsync(
        long sucursalId,
        CancellationToken ct = default) =>
        await DbSet
            .AsNoTracking()
            .Where(x => x.SucursalId == sucursalId && x.Activo)
            .OrderByDescending(x => x.EsDefault)
            .ThenBy(x => x.Descripcion)
            .ToListAsync(ct);

    public async Task<Deposito?> GetDefaultBySucursalAsync(
        long sucursalId,
        CancellationToken ct = default) =>
        await DbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.SucursalId == sucursalId &&
                x.EsDefault  == true       &&
                x.Activo     == true,
                ct);
}