using ZuluIA_Back.Domain.Entities.Facturacion;

namespace ZuluIA_Back.Domain.Interfaces;

public interface IPeriodoIvaRepository : IRepository<PeriodoIva>
{
    Task<IReadOnlyList<PeriodoIva>> GetBySucursalAsync(
        long sucursalId,
        long? ejercicioId,
        CancellationToken ct = default);

    Task<PeriodoIva?> GetPeriodoAsync(
        long sucursalId,
        DateOnly periodo,
        CancellationToken ct = default);

    Task<bool> EstaAbiertoPeriodoAsync(
        long sucursalId,
        DateOnly fecha,
        CancellationToken ct = default);
}