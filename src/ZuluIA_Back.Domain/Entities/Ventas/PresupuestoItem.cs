namespace ZuluIA_Back.Domain.Entities.Ventas;

using ZuluIA_Back.Domain.Common;

/// <summary>
/// Línea de ítem dentro de un presupuesto.
/// Mapea a la tabla presupuestos_items.
/// </summary>
public class PresupuestoItem : BaseEntity
{
    public long PresupuestoId    { get; private set; }
    public long ItemId           { get; private set; }
    public string Descripcion    { get; private set; } = null!;
    public decimal Cantidad      { get; private set; }
    public decimal PrecioUnitario { get; private set; }
    public decimal DescuentoPct  { get; private set; }
    public decimal Subtotal      { get; private set; }
    public short Orden           { get; private set; }

    private PresupuestoItem() { }

    public static PresupuestoItem Crear(
        long presupuestoId,
        long itemId,
        string descripcion,
        decimal cantidad,
        decimal precioUnitario,
        decimal descuentoPct,
        short orden = 0)
    {
        var subtotal = cantidad * precioUnitario * (1 - descuentoPct / 100);
        return new PresupuestoItem
        {
            PresupuestoId  = presupuestoId,
            ItemId         = itemId,
            Descripcion    = descripcion.Trim(),
            Cantidad       = cantidad,
            PrecioUnitario = precioUnitario,
            DescuentoPct   = descuentoPct,
            Subtotal       = Math.Round(subtotal, 4),
            Orden          = orden,
        };
    }
}
