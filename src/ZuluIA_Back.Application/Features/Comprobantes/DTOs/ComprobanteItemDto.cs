namespace ZuluIA_Back.Application.Features.Comprobantes.DTOs;

public class ComprobanteItemDto
{
    public long Id { get; set; }
    public long ItemId { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public decimal Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal DescuentoPct { get; set; }
    public decimal SubtotalNeto { get; set; }
    public decimal IvaImporte { get; set; }
    public decimal TotalLinea { get; set; }
    public short Orden { get; set; }
}