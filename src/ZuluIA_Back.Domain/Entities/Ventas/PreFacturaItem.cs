using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Ventas;

/// <summary>
/// Ítem de línea de una pre-factura de venta.
/// </summary>
public class PreFacturaItem : BaseEntity
{
    public long    PreFacturaId { get; private set; }
    public long    ItemId       { get; private set; }
    public decimal Cantidad     { get; private set; }
    public decimal PrecioUnitario { get; private set; }
    public decimal AlicuotaIva  { get; private set; }
    public decimal SubTotal     { get; private set; }
    public decimal MontoIva     { get; private set; }
    public string? Descripcion  { get; private set; }

    private PreFacturaItem() { }

    public static PreFacturaItem Crear(
        long preFacturaId, long itemId, decimal cantidad,
        decimal precioUnitario, decimal alicuotaIva, string? descripcion)
    {
        if (cantidad <= 0)        throw new InvalidOperationException("La cantidad debe ser mayor a 0.");
        if (precioUnitario < 0)   throw new InvalidOperationException("El precio no puede ser negativo.");

        var subTotal = cantidad * precioUnitario;
        var montoIva = subTotal * alicuotaIva / 100m;

        return new PreFacturaItem
        {
            PreFacturaId   = preFacturaId,
            ItemId         = itemId,
            Cantidad       = cantidad,
            PrecioUnitario = precioUnitario,
            AlicuotaIva    = alicuotaIva,
            SubTotal       = subTotal,
            MontoIva       = montoIva,
            Descripcion    = descripcion?.Trim()
        };
    }
}
