using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Domain.Entities.Facturacion;

public class AfipWsfeAudit : AuditableEntity
{
    public long ComprobanteId { get; private set; }
    public long SucursalId { get; private set; }
    public long PuntoFacturacionId { get; private set; }
    public TipoOperacionAfipWsfe Operacion { get; private set; }
    public bool Exitoso { get; private set; }
    public string RequestPayload { get; private set; } = string.Empty;
    public string ResponsePayload { get; private set; } = string.Empty;
    public string? MensajeError { get; private set; }
    public string? Cae { get; private set; }
    public string? Caea { get; private set; }
    public DateOnly FechaOperacion { get; private set; }

    private AfipWsfeAudit() { }

    public static AfipWsfeAudit Registrar(
        long comprobanteId,
        long sucursalId,
        long puntoFacturacionId,
        TipoOperacionAfipWsfe operacion,
        bool exitoso,
        string requestPayload,
        string responsePayload,
        string? mensajeError,
        string? cae,
        string? caea,
        DateOnly fechaOperacion,
        long? userId)
    {
        var audit = new AfipWsfeAudit
        {
            ComprobanteId = comprobanteId,
            SucursalId = sucursalId,
            PuntoFacturacionId = puntoFacturacionId,
            Operacion = operacion,
            Exitoso = exitoso,
            RequestPayload = requestPayload,
            ResponsePayload = responsePayload,
            MensajeError = mensajeError?.Trim(),
            Cae = cae?.Trim(),
            Caea = caea?.Trim(),
            FechaOperacion = fechaOperacion
        };

        audit.SetCreated(userId);
        return audit;
    }
}
