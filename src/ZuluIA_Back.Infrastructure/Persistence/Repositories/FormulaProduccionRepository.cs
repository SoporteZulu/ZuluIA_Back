using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Domain.Entities.Produccion;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Infrastructure.Persistence.Repositories;

public class FormulaProduccionRepository(AppDbContext context)
    : BaseRepository<FormulaProduccion>(context), IFormulaProduccionRepository
{
    public async Task<FormulaProduccion?> GetByIdConIngredientesAsync(
        long id,
        CancellationToken ct = default) =>
        await DbSet
            .Include(x => x.Ingredientes)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<IReadOnlyList<FormulaProduccion>> GetActivasAsync(
        CancellationToken ct = default) =>
        await DbSet
            .AsNoTracking()
            .Where(x => x.Activo)
            .OrderBy(x => x.Codigo)
            .ToListAsync(ct);

    public async Task<bool> ExisteCodigoAsync(
        string codigo,
        long? excludeId,
        CancellationToken ct = default)
    {
        var query = DbSet.Where(x =>
            x.Codigo == codigo.Trim().ToUpperInvariant());

        if (excludeId.HasValue)
            query = query.Where(x => x.Id != excludeId.Value);

        return await query.AnyAsync(ct);
    }
}