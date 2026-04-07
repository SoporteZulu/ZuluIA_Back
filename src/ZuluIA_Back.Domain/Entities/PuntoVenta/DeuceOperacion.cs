using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Domain.Entities.PuntoVenta;

public class DeuceOperacion : AuditableEntity
{
    public long ComprobanteId { get; private set; }
    public long SucursalId { get; private set; }
    public long PuntoFacturacionId { get; private set; }
    public EstadoIntegracionFiscalAlternativa Estado { get; private set; }
    public string ReferenciaExterna { get; private set; } = string.Empty;
    public string? RequestPayload { get; private set; }
    public string? ResponsePayload { get; private set; }
    public string? Observacion { get; private set; }

    private DeuceOperacion() { }

    public static DeuceOperacion Registrar(long comprobanteId, long sucursalId, long puntoFacturacionId, string referenciaExterna, string? requestPayload, string? responsePayload, string? observacion, bool confirmada, long? userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(referenciaExterna);
        var operacion = new DeuceOperacion
        {
            ComprobanteId = comprobanteId,
            SucursalId = sucursalId,
            PuntoFacturacionId = puntoFacturacionId,
            ReferenciaExterna = referenciaExterna.Trim().ToUpperInvariant(),
            RequestPayload = requestPayload?.Trim(),
            ResponsePayload = responsePayload?.Trim(),
            Observacion = observacion?.Trim(),
            Estado = confirmada ? EstadoIntegracionFiscalAlternativa.Confirmada : EstadoIntegracionFiscalAlternativa.Registrada
        };
        operacion.SetCreated(userId);
        return operacion;
    }

    public void Confirmar(string? observacion, long? userId)
    {
        Estado = EstadoIntegracionFiscalAlternativa.Confirmada;
        Observacion = observacion?.Trim() ?? Observacion;
        SetUpdated(userId);
    }

    public void Rechazar(string? observacion, long? userId)
    {
        Estado = EstadoIntegracionFiscalAlternativa.Rechazada;
        Observacion = observacion?.Trim() ?? Observacion;
        SetUpdated(userId);
    }
}
