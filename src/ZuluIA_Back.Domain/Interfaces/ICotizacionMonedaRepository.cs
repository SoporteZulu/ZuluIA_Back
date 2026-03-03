using ZuluIA_Back.Domain.Entities.Finanzas;

namespace ZuluIA_Back.Domain.Interfaces;

public interface ICotizacionMonedaRepository : IRepository<CotizacionMoneda>
{
    /// <summary>
    /// Retorna la cotización vigente de una moneda en una fecha dada.
    /// Busca la cotización más reciente menor o igual a la fecha.
    /// </summary>
    Task<CotizacionMoneda?> GetVigenteAsync(
        long monedaId,
        DateOnly fecha,
        CancellationToken ct = default);

    Task<IReadOnlyList<CotizacionMoneda>> GetHistoricoAsync(
        long monedaId,
        DateOnly? desde,
        DateOnly? hasta,
        CancellationToken ct = default);

    Task<bool> ExisteParaFechaAsync(
        long monedaId,
        DateOnly fecha,
        CancellationToken ct = default);
}