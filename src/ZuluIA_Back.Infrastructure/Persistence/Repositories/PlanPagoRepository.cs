using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Domain.Entities.Ventas;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Infrastructure.Persistence.Repositories;

public class PlanPagoRepository(AppDbContext context)
    : BaseRepository<PlanPago>(context), IPlanPagoRepository
{
    public async Task<IReadOnlyList<PlanPago>> GetActivosAsync(
        CancellationToken ct = default) =>
        await DbSet
            .AsNoTracking()
            .Where(x => x.Activo)
            .OrderBy(x => x.Descripcion)
            .ToListAsync(ct);
}