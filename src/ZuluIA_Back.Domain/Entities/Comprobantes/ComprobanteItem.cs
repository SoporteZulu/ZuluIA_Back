using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Enums;

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
    
    // Campos extendidos para paridad con zuluApp
    public string? Lote { get; private set; }
    public string? Serie { get; private set; }
    public DateOnly? FechaVencimiento { get; private set; }
    public long? UnidadMedidaId { get; private set; }
    public string? ObservacionRenglon { get; private set; }
    public decimal? PrecioListaOriginal { get; private set; }
    public decimal? ComisionVendedorRenglon { get; private set; }
    public long? ComprobanteItemOrigenId { get; private set; }
    
    // Campos específicos para notas de débito/crédito (referencias al documento origen)
    public decimal? CantidadDocumentoOrigen { get; private set; }
    public decimal? PrecioDocumentoOrigen { get; private set; }

    // Campos de pedido (cumplimiento)
    public decimal CantidadEntregada { get; private set; }
    public decimal CantidadPendiente { get; private set; }
    public EstadoEntregaItem? EstadoEntrega { get; private set; }
    public bool EsAtrasado { get; private set; }

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
        bool esGravado = true,
        string? lote = null,
        string? serie = null,
        DateOnly? fechaVencimiento = null,
        long? unidadMedidaId = null,
        string? observacionRenglon = null,
        decimal? precioListaOriginal = null,
        decimal? comisionVendedorRenglon = null,
        long? comprobanteItemOrigenId = null)
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
        var ivaImporte = esGravado ? baseNeta * (porcentajeIva / 100m) : 0;
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
            SubtotalNeto       = baseNeta,
            IvaImporte         = ivaImporte,
            TotalLinea         = totalLinea,
            DepositoId         = depositoId,
            Orden              = orden,
            EsGravado          = esGravado,
            Lote               = lote,
            Serie              = serie,
            FechaVencimiento   = fechaVencimiento,
            UnidadMedidaId     = unidadMedidaId,
            ObservacionRenglon = observacionRenglon,
            PrecioListaOriginal = precioListaOriginal,
            ComisionVendedorRenglon = comisionVendedorRenglon,
            ComprobanteItemOrigenId = comprobanteItemOrigenId,
            CantidadEntregada = 0,
            CantidadPendiente = cantidad,
            EstadoEntrega = EstadoEntregaItem.NoEntregado,
            EsAtrasado = false
        };
    }

    public void SetDatosDocumentoOrigen(decimal? cantidadOrigen, decimal? precioOrigen)
    {
        CantidadDocumentoOrigen = cantidadOrigen;
        PrecioDocumentoOrigen = precioOrigen;
    }

    public void RegistrarEntrega(decimal cantidadEntregadaAhora, DateOnly? fechaEntregaCompromiso)
    {
        if (cantidadEntregadaAhora <= 0)
            throw new InvalidOperationException("La cantidad entregada debe ser mayor a 0.");

        ActualizarCumplimiento(CantidadEntregada + cantidadEntregadaAhora, fechaEntregaCompromiso);
    }

    public void ActualizarCumplimiento(decimal cantidadEntregada, DateOnly? fechaEntregaCompromiso)
    {
        if (cantidadEntregada < 0)
            throw new InvalidOperationException("La cantidad entregada no puede ser negativa.");

        CantidadEntregada = cantidadEntregada;
        CantidadPendiente = Math.Max(0, Cantidad - CantidadEntregada);

        EstadoEntrega = CantidadEntregada switch
        {
            <= 0 => EstadoEntregaItem.NoEntregado,
            var c when c < Cantidad => EstadoEntregaItem.EntregaParcial,
            var c when c == Cantidad => EstadoEntregaItem.EntregaCompleta,
            _ => EstadoEntregaItem.EntregaSobrepasada
        };

        ActualizarEstadoAtraso(fechaEntregaCompromiso);
    }

    public void ActualizarEstadoAtraso(DateOnly? fechaEntregaCompromiso)
    {
        EsAtrasado = fechaEntregaCompromiso.HasValue
            && DateOnly.FromDateTime(DateTime.Today) > fechaEntregaCompromiso.Value
            && CantidadPendiente > 0;
    }

    public decimal ObtenerDiferencia() => Cantidad - CantidadEntregada;
}
