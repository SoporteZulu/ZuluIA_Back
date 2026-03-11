using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Domain.Entities.Contabilidad;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Infrastructure.Persistence.Repositories;

public class PlanCuentasRepository(AppDbContext context)
    : BaseRepository<PlanCuenta>(context), IPlanCuentasRepository
{
    public async Task<IReadOnlyList<PlanCuenta>> GetByEjercicioAsync(
        long ejercicioId,
        CancellationToken ct = default) =>
        await DbSet
            .AsNoTracking()
            .Where(x => x.EjercicioId == ejercicioId)
            .OrderBy(x => x.OrdenNivel)
            .ThenBy(x => x.CodigoCuenta)
            .ToListAsync(ct);

    public async Task<PlanCuenta?> GetByCodigoAsync(
        long ejercicioId,
        string codigo,
        CancellationToken ct = default) =>
        await DbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.EjercicioId == ejercicioId &&
                x.CodigoCuenta == codigo.Trim(),
                ct);

    public async Task<IReadOnlyList<PlanCuenta>> GetImputablesAsync(
        long ejercicioId,
        CancellationToken ct = default) =>
        await DbSet
            .AsNoTracking()
            .Where(x => x.EjercicioId == ejercicioId && x.Imputable)
            .OrderBy(x => x.CodigoCuenta)
            .ToListAsync(ct);

    public async Task<bool> ExisteCodigoAsync(
        long ejercicioId,
        string codigo,
        long? excludeId,
        CancellationToken ct = default)
    {
        var query = DbSet.Where(x =>
            x.EjercicioId  == ejercicioId &&
            x.CodigoCuenta == codigo.Trim());

        if (excludeId.HasValue)
            query = query.Where(x => x.Id != excludeId.Value);

        return await query.AnyAsync(ct);
    }
}