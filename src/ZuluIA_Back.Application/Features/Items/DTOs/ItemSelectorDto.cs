namespace ZuluIA_Back.Application.Features.Items.DTOs;

/// <summary>
/// DTO mínimo para combos, selectores y autocomplete comercial de productos.
/// </summary>
public class ItemSelectorDto
{
    public long Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string? CodigoBarras { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public long UnidadMedidaId { get; set; }
    public string? UnidadMedidaDescripcion { get; set; }
    public long AlicuotaIvaId { get; set; }
    public decimal PrecioVenta { get; set; }
    public decimal StockDisponible { get; set; }
    public decimal? PorcentajeMaximoDescuento { get; set; }
    public bool ManejaStock { get; set; }
    public bool EsVendible { get; set; }
    public string Display => $"{Codigo} - {Descripcion}";
}