using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Logistica;

/// <summary>Línea de detalle de una Orden de Empaque.</summary>
public class OrdenEmpaqueDetalle : BaseEntity
{
    public long OrdenEmpaqueId { get; private set; }
    public long? ItemId { get; private set; }
    public string Descripcion { get; private set; } = string.Empty;
    public decimal Cantidad { get; private set; }
    public decimal PrecioUnitario { get; private set; }
    public decimal? PorcentajeIva { get; private set; }
    public decimal Total { get; private set; }
    public string? Observacion { get; private set; }

    private OrdenEmpaqueDetalle() { }

    public static OrdenEmpaqueDetalle Crear(
        long ordenEmpaqueId,
        long? itemId,
        string descripcion,
        decimal cantidad,
        decimal precioUnitario,
        decimal? porcentajeIva,
        string? observacion)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(descripcion);
        if (cantidad <= 0) throw new ArgumentException("La cantidad debe ser mayor a 0.");

        return new OrdenEmpaqueDetalle
        {
            OrdenEmpaqueId = ordenEmpaqueId,
            ItemId         = itemId,
            Descripcion    = descripcion.Trim(),
            Cantidad       = cantidad,
            PrecioUnitario = precioUnitario,
            PorcentajeIva  = porcentajeIva,
            Total          = cantidad * precioUnitario,
            Observacion    = observacion?.Trim()
        };
    }
}
