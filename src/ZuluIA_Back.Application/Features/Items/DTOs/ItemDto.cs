namespace ZuluIA_Back.Application.Features.Items.DTOs;

public class ItemDto
{
    public long Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string? CodigoBarras { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public string? DescripcionAdicional { get; set; }
    public long? CategoriaId { get; set; }
    public long UnidadMedidaId { get; set; }
    public long AlicuotaIvaId { get; set; }
    public long MonedaId { get; set; }
    public bool EsProducto { get; set; }
    public bool EsServicio { get; set; }
    public bool ManejaStock { get; set; }
    public decimal PrecioCosto { get; set; }
    public decimal PrecioVenta { get; set; }
    public decimal StockMinimo { get; set; }
    public decimal? StockMaximo { get; set; }
    public string? CodigoAfip { get; set; }
    public bool Activo { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}