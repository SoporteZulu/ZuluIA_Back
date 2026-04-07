using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Domain.Entities.PuntoVenta;

public class SifenOperacion : AuditableEntity
{
    public long ComprobanteId { get; private set; }
    public long SucursalId { get; private set; }
    public long PuntoFacturacionId { get; private set; }
    public long? TimbradoFiscalId { get; private set; }
    public EstadoIntegracionFiscalAlternativa Estado { get; private set; }
    public string? RequestPayload { get; private set; }
    public string? ResponsePayload { get; private set; }
    public string? CodigoSeguridad { get; private set; }
    public string? Observacion { get; private set; }

    private SifenOperacion() { }

    public static SifenOperacion Registrar(long comprobanteId, long sucursalId, long puntoFacturacionId, long? timbradoFiscalId, string? requestPayload, string? responsePayload, string? codigoSeguridad, string? observacion, bool confirmada, long? userId)
    {
        var operacion = new SifenOperacion
        {
            ComprobanteId = comprobanteId,
            SucursalId = sucursalId,
            PuntoFacturacionId = puntoFacturacionId,
            TimbradoFiscalId = timbradoFiscalId,
            RequestPayload = requestPayload?.Trim(),
            ResponsePayload = responsePayload?.Trim(),
            CodigoSeguridad = codigoSeguridad?.Trim(),
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
