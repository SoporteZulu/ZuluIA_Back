using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Domain.Services;

/// <summary>
/// Servicio de dominio que resuelve la numeración automática de comprobantes.
/// Encapsula la lógica de negocio de numeración independiente de la infraestructura.
/// </summary>
public class NumeracionComprobanteService(IPuntoFacturacionRepository puntoRepo)
{
    /// <summary>
    /// Obtiene el próximo número de comprobante para un punto de facturación y tipo.
    /// </summary>
    public async Task<(short Prefijo, long Numero)> ObtenerProximoNumeroAsync(
        long puntoFacturacionId,
        long tipoComprobanteId,
        CancellationToken ct = default)
    {
        var punto = await puntoRepo.GetByIdAsync(puntoFacturacionId, ct)
            ?? throw new InvalidOperationException(
                $"No se encontró el punto de facturación con ID {puntoFacturacionId}.");

        if (!punto.Activo)
            throw new InvalidOperationException(
                $"El punto de facturación {punto.Numero} no está activo.");

        var proximoNumero = await puntoRepo.GetProximoNumeroComprobanteAsync(
            puntoFacturacionId, tipoComprobanteId, ct);

        return (punto.Numero, proximoNumero);
    }
}