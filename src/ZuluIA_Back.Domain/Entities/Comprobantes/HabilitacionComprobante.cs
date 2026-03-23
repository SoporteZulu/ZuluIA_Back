using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Domain.Entities.Comprobantes;

/// <summary>
/// Proceso de habilitación/desbloqueo de comprobantes de compra y órdenes de compra retenidos.
/// Migrado desde VB6: HABILITACION, HABILITARCOMPROBANTECOMPRA, HABILITARORDENCOMPRA.
/// </summary>
public class HabilitacionComprobante : AuditableEntity
{
    public long               ComprobanteId   { get; private set; }
    public long               SucursalId      { get; private set; }
    public string             TipoDocumento   { get; private set; } = string.Empty; // COMPROBANTE | ORDEN_COMPRA
    public EstadoHabilitacion Estado          { get; private set; }
    public string?            MotivoBloqueo   { get; private set; }
    public string?            ObservacionHabilitacion { get; private set; }
    public long?              HabilitadoPor   { get; private set; }
    public DateTimeOffset?    FechaHabilitacion { get; private set; }

    private HabilitacionComprobante() { }

    public static HabilitacionComprobante Crear(
        long comprobanteId, long sucursalId, string tipoDocumento,
        string? motivoBloqueo, long? userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tipoDocumento);
        var h = new HabilitacionComprobante
        {
            ComprobanteId = comprobanteId,
            SucursalId    = sucursalId,
            TipoDocumento = tipoDocumento.Trim().ToUpperInvariant(),
            MotivoBloqueo = motivoBloqueo?.Trim(),
            Estado        = EstadoHabilitacion.Pendiente
        };
        h.SetCreated(userId);
        return h;
    }

    public void Habilitar(long habilitadoPor, string? observacion, long? userId)
    {
        if (Estado != EstadoHabilitacion.Pendiente)
            throw new InvalidOperationException("Solo se pueden habilitar solicitudes pendientes.");
        Estado                  = EstadoHabilitacion.Habilitado;
        HabilitadoPor           = habilitadoPor;
        ObservacionHabilitacion = observacion?.Trim();
        FechaHabilitacion       = DateTimeOffset.UtcNow;
        SetUpdated(userId);
    }

    public void Rechazar(long habilitadoPor, string? observacion, long? userId)
    {
        if (Estado != EstadoHabilitacion.Pendiente)
            throw new InvalidOperationException("Solo se pueden rechazar solicitudes pendientes.");
        Estado                  = EstadoHabilitacion.Rechazado;
        HabilitadoPor           = habilitadoPor;
        ObservacionHabilitacion = observacion?.Trim();
        FechaHabilitacion       = DateTimeOffset.UtcNow;
        SetUpdated(userId);
    }
}
