namespace ZuluIA_Back.Application.Features.Items.DTOs;

public class ItemListDto
{
    public long Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string? CodigoBarras { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public long? CategoriaId { get; set; }
    public string? CategoriaDescripcion { get; set; }
    public long? MarcaId { get; set; }
    public string? MarcaDescripcion { get; set; }
    public long UnidadMedidaId { get; set; }
    public string? UnidadMedidaDescripcion { get; set; }
    public bool EsProducto { get; set; }
    public bool EsServicio { get; set; }
    public bool ManejaStock { get; set; }
    public decimal PrecioVenta { get; set; }
    public bool Activo { get; set; }
}
