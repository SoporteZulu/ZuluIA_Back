using ZuluIA_Back.Domain.Entities.Facturacion;

namespace ZuluIA_Back.Domain.Interfaces;

public interface IPuntoFacturacionRepository : IRepository<PuntoFacturacion>
{
    Task<IReadOnlyList<PuntoFacturacion>> GetActivosBySucursalAsync(
        long sucursalId,
        CancellationToken ct = default);

    Task<bool> ExisteNumeroAsync(
        long sucursalId,
        short numero,
        long? excludeId = null,
        CancellationToken ct = default);

    /// <summary>
    /// Obtiene el próximo número de comprobante para un punto y tipo.
    /// </summary>
    Task<long> GetProximoNumeroComprobanteAsync(
        long puntoFacturacionId,
        long tipoComprobanteId,
        CancellationToken ct = default);
}