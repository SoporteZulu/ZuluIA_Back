using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Ventas;

/// <summary>
/// Ítem/línea de un contrato de servicio con datos de facturación periódica.
/// Tabla VB6: CONTRATOSDETALLES
/// </summary>
public class ContratoDetalle : BaseEntity
{
    public long ContratoId { get; private set; }
    public long? ItemId { get; private set; }
    public string Descripcion { get; private set; } = string.Empty;
    public decimal Cantidad { get; private set; }
    public decimal PrecioUnitario { get; private set; }
    public decimal? PorcentajeIva { get; private set; }
    public decimal Total { get; private set; }

    /// <summary>Fecha de vigencia desde.</summary>
    public DateOnly FechaDesde { get; private set; }
    /// <summary>Fecha de baja/vencimiento del ítem.</summary>
    public DateOnly FechaHasta { get; private set; }
    /// <summary>Fecha en que se emitirá la primera factura.</summary>
    public DateOnly FechaPrimeraFactura { get; private set; }
    /// <summary>Frecuencia de facturación en meses.</summary>
    public int FrecuenciaMeses { get; private set; }
    /// <summary>Día de corte para facturación proporcional.</summary>
    public int Corte { get; private set; }
    /// <summary>Dominio o matrícula (ej. vehículo registrado en el contrato).</summary>
    public string? Dominio { get; private set; }
    public string Estado { get; private set; } = "ACTIVO";
    public bool Anulado { get; private set; } = false;

    private ContratoDetalle() { }

    internal static ContratoDetalle Crear(
        long contratoId,
        long? itemId,
        string descripcion,
        decimal cantidad,
        decimal precioUnitario,
        decimal? porcentajeIva,
        DateOnly fechaDesde,
        DateOnly fechaHasta,
        DateOnly fechaPrimeraFactura,
        int frecuenciaMeses,
        int corte,
        string? dominio)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(descripcion);
        if (cantidad <= 0) throw new ArgumentException("La cantidad debe ser mayor a 0.");
        if (precioUnitario < 0) throw new ArgumentException("El precio no puede ser negativo.");

        return new ContratoDetalle
        {
            ContratoId         = contratoId,
            ItemId             = itemId,
            Descripcion        = descripcion.Trim(),
            Cantidad           = cantidad,
            PrecioUnitario     = precioUnitario,
            PorcentajeIva      = porcentajeIva,
            Total              = cantidad * precioUnitario,
            FechaDesde         = fechaDesde,
            FechaHasta         = fechaHasta,
            FechaPrimeraFactura = fechaPrimeraFactura,
            FrecuenciaMeses    = frecuenciaMeses,
            Corte              = corte,
            Dominio            = dominio?.Trim().ToUpperInvariant(),
            Estado             = "ACTIVO",
            Anulado            = false
        };
    }

    public void Anular() => (Anulado, Estado) = (true, "ANULADO");
}
