using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Items;

namespace ZuluIA_Back.Domain.Interfaces;

public interface ICategoriaItemRepository : IRepository<CategoriaItem>
{
    Task<IReadOnlyList<CategoriaItem>> GetArbolCompletoAsync(
        CancellationToken ct = default);

    Task<IReadOnlyList<CategoriaItem>> GetByNivelAsync(
        short nivel,
        CancellationToken ct = default);

    Task<bool> ExisteCodigoAsync(
        string codigo,
        long? excludeId = null,
        CancellationToken ct = default);
}