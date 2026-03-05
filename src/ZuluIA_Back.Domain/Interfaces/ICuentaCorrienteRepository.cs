using ZuluIA_Back.Domain.Entities.Finanzas;

namespace ZuluIA_Back.Domain.Interfaces;

public interface ICuentaCorrienteRepository : IRepository<CuentaCorriente>
{
    Task<CuentaCorriente?> GetByTerceroMonedaAsync(
        long terceroId,
        long monedaId,
        long? sucursalId,
        CancellationToken ct = default);

    Task<CuentaCorriente> GetOrCreateAsync(
        long terceroId,
        long monedaId,
        long? sucursalId,
        CancellationToken ct = default);

    Task<IReadOnlyList<CuentaCorriente>> GetByTerceroAsync(
        long terceroId,
        CancellationToken ct = default);
}