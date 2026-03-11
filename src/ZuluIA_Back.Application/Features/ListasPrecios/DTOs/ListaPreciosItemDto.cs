namespace ZuluIA_Back.Application.Features.ListasPrecios.DTOs;

public class ListaPreciosItemDto
{
    public long Id { get; set; }
    public long ListaId { get; set; }
    public long ItemId { get; set; }
    public decimal Precio { get; set; }
    public decimal DescuentoPct { get; set; }
    public decimal PrecioFinal { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}