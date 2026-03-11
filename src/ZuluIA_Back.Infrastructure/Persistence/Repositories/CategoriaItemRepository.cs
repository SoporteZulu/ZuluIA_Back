using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Domain.Entities.Items;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Infrastructure.Persistence.Repositories;

public class CategoriaItemRepository(AppDbContext context)
    : BaseRepository<CategoriaItem>(context), ICategoriaItemRepository
{
    public async Task<IReadOnlyList<CategoriaItem>> GetArbolCompletoAsync(
        CancellationToken ct = default) =>
        await DbSet
            .AsNoTracking()
            .Where(x => x.Activo)
            .OrderBy(x => x.Nivel)
            .ThenBy(x => x.OrdenNivel)
            .ThenBy(x => x.Descripcion)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<CategoriaItem>> GetByNivelAsync(
        short nivel,
        CancellationToken ct = default) =>
        await DbSet
            .AsNoTracking()
            .Where(x => x.Nivel == nivel && x.Activo)
            .OrderBy(x => x.OrdenNivel)
            .ThenBy(x => x.Descripcion)
            .ToListAsync(ct);

    public async Task<bool> ExisteCodigoAsync(
        string codigo,
        long? excludeId = null,
        CancellationToken ct = default)
    {
        var query = DbSet.Where(x =>
            x.Codigo == codigo.Trim().ToUpperInvariant());

        if (excludeId.HasValue)
            query = query.Where(x => x.Id != excludeId.Value);

        return await query.AnyAsync(ct);
    }
}