using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Comprobantes;

public class ComprobanteItem : BaseEntity
{
    public long ComprobanteId { get; private set; }
    public long ItemId { get; private set; }
    public string Descripcion { get; private set; } = string.Empty;
    public decimal Cantidad { get; private set; }
    public decimal CantidadBonif { get; private set; }
    public decimal PrecioUnitario { get; private set; }
    public decimal DescuentoPct { get; private set; }
    public long AlicuotaIvaId { get; private set; }
    public decimal SubtotalNeto { get; private set; }
    public decimal IvaImporte { get; private set; }
    public decimal TotalLinea { get; private set; }
    public long? DepositoId { get; private set; }
    public short Orden { get; private set; }

    private ComprobanteItem() { }

    public static ComprobanteItem Crear(
        long comprobanteId,
        long itemId,
        string descripcion,
        decimal cantidad,
        decimal precioUnitario,
        decimal descuentoPct,
        long alicuotaIvaId,
        decimal porcentajeIva,
        long? depositoId,
        short orden,
        decimal cantidadBonif = 0)
    {
        if (cantidad <= 0)
            throw new InvalidOperationException("La cantidad debe ser mayor a 0.");
        if (precioUnitario < 0)
            throw new InvalidOperationException("El precio unitario no puede ser negativo.");

        var cantidadEfectiva = cantidad - cantidadBonif;
        var importeBruto = cantidadEfectiva * precioUnitario;
        var descuento = importeBruto * (descuentoPct / 100);
        var subtotalNeto = importeBruto - descuento;
        var ivaImporte = subtotalNeto * (porcentajeIva / 100);
        var totalLinea = subtotalNeto + ivaImporte;

        return new ComprobanteItem
        {
            ComprobanteId  = comprobanteId,
            ItemId         = itemId,
            Descripcion    = descripcion.Trim(),
            Cantidad       = cantidad,
            CantidadBonif  = cantidadBonif,
            PrecioUnitario = precioUnitario,
            DescuentoPct   = descuentoPct,
            AlicuotaIvaId  = alicuotaIvaId,
            SubtotalNeto   = Math.Round(subtotalNeto, 2),
            IvaImporte     = Math.Round(ivaImporte, 2),
            TotalLinea     = Math.Round(totalLinea, 2),
            DepositoId     = depositoId,
            Orden          = orden
        };
    }
}