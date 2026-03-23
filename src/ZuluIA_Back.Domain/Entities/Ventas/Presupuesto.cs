namespace ZuluIA_Back.Domain.Entities.Ventas;

using ZuluIA_Back.Domain.Common;

/// <summary>
/// Presupuesto / cotización comercial emitida a un cliente.
/// Puede ser convertida en un comprobante definitivo.
/// Mapea a la tabla presupuestos.
/// </summary>
public class Presupuesto : AuditableEntity
{
    public long SucursalId    { get; private set; }
    public long TerceroId     { get; private set; }
    public DateOnly Fecha     { get; private set; }
    public DateOnly? FechaVigencia { get; private set; }
    public long MonedaId      { get; private set; }
    public decimal Cotizacion { get; private set; } = 1;
    public decimal Total      { get; private set; }
    public string Estado      { get; private set; } = "PENDIENTE";
    public string? Observacion { get; private set; }
    /// <summary>Comprobante generado a partir de este presupuesto.</summary>
    public long? ComprobanteId { get; private set; }

    private readonly List<PresupuestoItem> _items = [];
    public IReadOnlyCollection<PresupuestoItem> Items => _items.AsReadOnly();

    private Presupuesto() { }

    public static Presupuesto Crear(
        long sucursalId,
        long terceroId,
        DateOnly fecha,
        DateOnly? fechaVigencia,
        long monedaId,
        decimal cotizacion,
        string? observacion,
        long? userId)
    {
        var p = new Presupuesto
        {
            SucursalId    = sucursalId,
            TerceroId     = terceroId,
            Fecha         = fecha,
            FechaVigencia = fechaVigencia,
            MonedaId      = monedaId,
            Cotizacion    = cotizacion <= 0 ? 1 : cotizacion,
            Estado        = "PENDIENTE",
            Observacion   = observacion?.Trim(),
        };
        p.SetCreated(userId);
        return p;
    }

    public void Aprobar(long? userId)
    {
        Estado = "APROBADO";
        SetUpdated(userId);
    }

    public void Rechazar(long? userId)
    {
        Estado = "RECHAZADO";
        SetUpdated(userId);
    }

    public void Convertir(long comprobanteId, long? userId)
    {
        ComprobanteId = comprobanteId;
        Estado        = "CONVERTIDO";
        SetUpdated(userId);
    }

    public void ActualizarTotal(decimal total, long? userId)
    {
        Total = total;
        SetUpdated(userId);
    }

    public void Eliminar()
    {
        SetDeleted();
    }
}
