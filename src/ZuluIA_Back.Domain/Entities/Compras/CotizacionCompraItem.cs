using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Compras;

public class CotizacionCompraItem : BaseEntity
{
    public long CotizacionId { get; private set; }
    public long? ItemId { get; private set; }
    public string Descripcion { get; private set; } = string.Empty;
    public decimal Cantidad { get; private set; }
    public decimal PrecioUnitario { get; private set; }
    public decimal Total { get; private set; }

    private CotizacionCompraItem() { }

    public static CotizacionCompraItem Crear(
        long cotizacionId,
        long? itemId,
        string descripcion,
        decimal cantidad,
        decimal precioUnitario)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(cantidad);
        ArgumentOutOfRangeException.ThrowIfNegative(precioUnitario);
        return new CotizacionCompraItem
        {
            CotizacionId   = cotizacionId,
            ItemId         = itemId,
            Descripcion    = descripcion.Trim(),
            Cantidad       = cantidad,
            PrecioUnitario = precioUnitario,
            Total          = cantidad * precioUnitario
        };
    }
}
