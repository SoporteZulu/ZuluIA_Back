namespace ZuluIA_Back.Application.Features.ListasPrecios.DTOs;

public class PrecioListaResueltoDto
{
    public decimal Precio { get; set; }
    public decimal DescuentoPct { get; set; }
    public decimal PrecioFinal { get; set; }
    public long? ListaPreciosId { get; set; }
    public string Origen { get; set; } = string.Empty;
    public long? PromocionId { get; set; }
}
