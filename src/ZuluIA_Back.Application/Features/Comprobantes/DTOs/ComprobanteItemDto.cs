namespace ZuluIA_Back.Application.Features.Comprobantes.DTOs;

using ZuluIA_Back.Domain.Enums;

public class ComprobanteItemDto
{
    public long Id { get; set; }
    public long ItemId { get; set; }
    public string ItemCodigo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public decimal Cantidad { get; set; }
    public decimal CantidadBonificada { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal DescuentoPct { get; set; }
    public decimal Descuento => DescuentoPct;
    public long AlicuotaIvaId { get; set; }
    public decimal PorcentajeIva { get; set; }
    public decimal AlicuotaIvaPct => PorcentajeIva;
    public decimal SubtotalNeto { get; set; }
    public decimal Subtotal => SubtotalNeto;
    public decimal IvaImporte { get; set; }
    public decimal TotalLinea { get; set; }
    public long? DepositoId { get; set; }
    public string? DepositoDescripcion { get; set; }
    public short Orden { get; set; }
    public bool EsGravado { get; set; }
    
    // Campos extendidos para paridad con zuluApp
    public string? Lote { get; set; }
    public string? Serie { get; set; }
    public DateOnly? FechaVencimiento { get; set; }
    public long? UnidadMedidaId { get; set; }
    public string? UnidadMedidaDescripcion { get; set; }
    public string? ObservacionRenglon { get; set; }
    public decimal? PrecioListaOriginal { get; set; }
    public decimal? ComisionVendedorRenglon { get; set; }
    public long? ComprobanteItemOrigenId { get; set; }
    
    // Campos específicos para notas de débito/crédito (referencias al documento origen)
    public decimal? CantidadDocumentoOrigen { get; set; }
    public decimal? PrecioDocumentoOrigen { get; set; }
    public decimal? DiferenciaCantidad => CantidadDocumentoOrigen.HasValue 
        ? Cantidad - CantidadDocumentoOrigen.Value 
        : null;
    public decimal? DiferenciaPrecio => PrecioDocumentoOrigen.HasValue 
        ? PrecioUnitario - PrecioDocumentoOrigen.Value 
        : null;
    
    // Campos de cumplimiento de pedidos y seguimiento de entrega
    public decimal CantidadEntregada { get; set; }
    public decimal CantidadPendiente { get; set; }
    public EstadoEntregaItem? EstadoEntrega { get; set; }
    public string? EstadoEntregaDescripcion { get; set; }
    public bool EsAtrasado { get; set; }
    public decimal Diferencia { get; set; }
    
    public IReadOnlyList<ComprobanteItemAtributoDto> Atributos { get; set; } = [];
}
