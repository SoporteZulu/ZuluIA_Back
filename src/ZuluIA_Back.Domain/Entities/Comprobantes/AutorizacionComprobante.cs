using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Domain.Entities.Comprobantes;

/// <summary>
/// Workflow de autorización para comprobantes de venta o compra que requieren aprobación.
/// Migrado desde VB6: VtaAUTORIZACION, VtaAUTORIZACIONF, LtoCprAUTORIZACION.
/// </summary>
public class AutorizacionComprobante : AuditableEntity
{
    public long               ComprobanteId   { get; private set; }
    public long               SucursalId      { get; private set; }
    public string             TipoOperacion   { get; private set; } = string.Empty; // VENTA | COMPRA
    public EstadoAutorizacion Estado          { get; private set; }
    public string?            Motivo          { get; private set; }
    public long?              AutorizadoPor   { get; private set; }
    public DateTimeOffset?    FechaResolucion { get; private set; }

    private AutorizacionComprobante() { }

    public static AutorizacionComprobante Crear(
        long comprobanteId, long sucursalId, string tipoOperacion, long? userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tipoOperacion);
        var a = new AutorizacionComprobante
        {
            ComprobanteId = comprobanteId,
            SucursalId    = sucursalId,
            TipoOperacion = tipoOperacion.Trim().ToUpperInvariant(),
            Estado        = EstadoAutorizacion.Pendiente
        };
        a.SetCreated(userId);
        return a;
    }

    public void Autorizar(long autorizadoPor, string? motivo, long? userId)
    {
        if (Estado != EstadoAutorizacion.Pendiente)
            throw new InvalidOperationException("Solo se pueden autorizar solicitudes pendientes.");
        Estado          = EstadoAutorizacion.Autorizado;
        AutorizadoPor   = autorizadoPor;
        Motivo          = motivo?.Trim();
        FechaResolucion = DateTimeOffset.UtcNow;
        SetUpdated(userId);
    }

    public void Rechazar(long autorizadoPor, string? motivo, long? userId)
    {
        if (Estado != EstadoAutorizacion.Pendiente)
            throw new InvalidOperationException("Solo se pueden rechazar solicitudes pendientes.");
        Estado          = EstadoAutorizacion.Rechazado;
        AutorizadoPor   = autorizadoPor;
        Motivo          = motivo?.Trim();
        FechaResolucion = DateTimeOffset.UtcNow;
        SetUpdated(userId);
    }
}
