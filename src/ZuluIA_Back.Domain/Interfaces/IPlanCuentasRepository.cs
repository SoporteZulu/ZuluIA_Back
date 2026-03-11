using ZuluIA_Back.Domain.Entities.Contabilidad;

namespace ZuluIA_Back.Domain.Interfaces;

public interface IPlanCuentasRepository : IRepository<PlanCuenta>
{
    Task<IReadOnlyList<PlanCuenta>> GetByEjercicioAsync(
        long ejercicioId,
        CancellationToken ct = default);

    Task<PlanCuenta?> GetByCodigoAsync(
        long ejercicioId,
        string codigo,
        CancellationToken ct = default);

    Task<IReadOnlyList<PlanCuenta>> GetImputablesAsync(
        long ejercicioId,
        CancellationToken ct = default);

    Task<bool> ExisteCodigoAsync(
        long ejercicioId,
        string codigo,
        long? excludeId,
        CancellationToken ct = default);
}