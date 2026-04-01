namespace ZuluIA_Back.Application.Features.Items.DTOs;

/// <summary>
/// DTO liviano para resolución de precios en comprobantes.
/// </summary>
public class ItemPrecioDto
{
    public long Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public long UnidadMedidaId { get; set; }
    public long AlicuotaIvaId { get; set; }
    public decimal AlicuotaIvaPorcentaje { get; set; }
    public decimal PrecioCosto { get; set; }
    public decimal PrecioVenta { get; set; }
    public long MonedaId { get; set; }
    public bool ManejaStock { get; set; }
    public bool Activo { get; set; }
    public bool AplicaVentas { get; set; }
    public bool EsVendible { get; set; }
    public decimal Stock { get; set; }
    public decimal StockDisponible { get; set; }
    public decimal StockComprometido { get; set; }
    public decimal StockReservado { get; set; }
}