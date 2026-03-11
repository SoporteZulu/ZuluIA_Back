using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Produccion;

namespace ZuluIA_Back.Domain.Interfaces;

public interface IFormulaProduccionRepository : IRepository<FormulaProduccion>
{
    Task<FormulaProduccion?> GetByIdConIngredientesAsync(
        long id,
        CancellationToken ct = default);

    Task<IReadOnlyList<FormulaProduccion>> GetActivasAsync(
        CancellationToken ct = default);

    Task<bool> ExisteCodigoAsync(
        string codigo,
        long? excludeId,
        CancellationToken ct = default);
}