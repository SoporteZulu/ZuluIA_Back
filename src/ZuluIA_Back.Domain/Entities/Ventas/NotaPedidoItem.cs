using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Ventas;

public class NotaPedidoItem : BaseEntity
{
    public long NotaPedidoId { get; private set; }
    public long ItemId { get; private set; }
    public decimal Cantidad { get; private set; }
    public decimal CantidadPendiente { get; private set; }
    public decimal PrecioUnitario { get; private set; }
    public decimal Bonificacion { get; private set; }
    public decimal Subtotal { get; private set; }

    private NotaPedidoItem() { }

    public static NotaPedidoItem Crear(
        long notaPedidoId,
        long itemId,
        decimal cantidad,
        decimal precioUnitario,
        decimal bonificacion)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(cantidad);
        ArgumentOutOfRangeException.ThrowIfNegative(precioUnitario);
        var subtotal = cantidad * precioUnitario * (1 - bonificacion / 100);
        return new NotaPedidoItem
        {
            NotaPedidoId      = notaPedidoId,
            ItemId            = itemId,
            Cantidad          = cantidad,
            CantidadPendiente = cantidad,
            PrecioUnitario    = precioUnitario,
            Bonificacion      = bonificacion,
            Subtotal          = subtotal
        };
    }

    public void ReducirPendiente(decimal cantidadVinculada)
    {
        if (cantidadVinculada > CantidadPendiente)
            throw new InvalidOperationException("La cantidad vinculada supera la cantidad pendiente.");
        CantidadPendiente -= cantidadVinculada;
    }
}
