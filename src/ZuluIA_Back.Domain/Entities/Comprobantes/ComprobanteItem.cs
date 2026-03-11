using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Comprobantes;

public class ComprobanteItem : BaseEntity
{
    public long ComprobanteId { get; private set; }
    public long ItemId { get; private set; }
    public string Descripcion { get; private set; } = string.Empty;
    public decimal Cantidad { get; private set; }
    public long CantidadBonif { get; private set; } // Alias para compatibilidad
    public long CantidadBonificada { get; private set; }
    public long PrecioUnitario { get; private set; }
    public decimal DescuentoPct { get; private set; }
    public long AlicuotaIvaId { get; private set; }
    public long PorcentajeIva { get; private set; }
    public decimal SubtotalNeto { get; private set; }
    public decimal IvaImporte { get; private set; }
    public decimal TotalLinea { get; private set; }
    public long? DepositoId { get; private set; }
    public short Orden { get; private set; }
    public bool EsGravado { get; private set; } = true;

    private ComprobanteItem() { }

    public static ComprobanteItem Crear(
        long comprobanteId,
        long itemId,
        string descripcion,
        decimal cantidad,
        long cantidadBonificada,
        long precioUnitario,
        decimal descuentoPct,
        long alicuotaIvaId,
        long porcentajeIva,
        long? depositoId,
        short orden,
        bool esGravado = true)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(descripcion);

        if (cantidad <= 0)
            throw new InvalidOperationException("La cantidad debe ser mayor a 0.");

        if (precioUnitario < 0)
            throw new InvalidOperationException("El precio unitario no puede ser negativo.");

        var cantidadEfectiva = cantidad - cantidadBonificada;
        var importeBruto = cantidadEfectiva * precioUnitario;
        var descuento = importeBruto * (descuentoPct / 100);
        var baseNeta = importeBruto - descuento;
        var ivaImporte = esGravado ? baseNeta * (porcentajeIva / 100) : 0;
        var totalLinea = baseNeta + ivaImporte;

        return new ComprobanteItem
        {
            ComprobanteId      = comprobanteId,
            ItemId             = itemId,
            Descripcion        = descripcion.Trim(),
            Cantidad           = cantidad,
            CantidadBonif      = cantidadBonificada, // Alias para compatibilidad
            CantidadBonificada = cantidadBonificada,
            PrecioUnitario     = precioUnitario,
            DescuentoPct       = descuentoPct,
            AlicuotaIvaId      = alicuotaIvaId,
            PorcentajeIva      = porcentajeIva,
            SubtotalNeto       = Math.Round(baseNeta, 2),
            IvaImporte         = Math.Round(ivaImporte, 2),
            TotalLinea         = Math.Round(totalLinea, 2),
            DepositoId         = depositoId,
            Orden              = orden,
            EsGravado          = esGravado
        };
    }
}
