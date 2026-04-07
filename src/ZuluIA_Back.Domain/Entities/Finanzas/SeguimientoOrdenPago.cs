using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Finanzas;

/// <summary>
/// Seguimiento del estado de una orden de pago (aprobaciones, pagos parciales, etc.).
/// Migrado desde VB6: SEGUIMIENTOORDENPAGO.
/// </summary>
public class SeguimientoOrdenPago : AuditableEntity
{
    public long    PagoId       { get; private set; }
    public long    SucursalId   { get; private set; }
    public DateOnly Fecha       { get; private set; }
    public string  Estado       { get; private set; } = string.Empty;
    public string? Observacion  { get; private set; }
    public long?   UsuarioId    { get; private set; }

    private SeguimientoOrdenPago() { }

    public static SeguimientoOrdenPago Registrar(
        long pagoId, long sucursalId, DateOnly fecha, string estado,
        string? observacion, long? userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(estado);
        var s = new SeguimientoOrdenPago
        {
            PagoId      = pagoId,
            SucursalId  = sucursalId,
            Fecha       = fecha,
            Estado      = estado.Trim().ToUpperInvariant(),
            Observacion = observacion?.Trim(),
            UsuarioId   = userId
        };
        s.SetCreated(userId);
        return s;
    }

    public void ActualizarObservacion(string? obs, long? userId)
    {
        Observacion = obs?.Trim();
        SetUpdated(userId);
    }
}
