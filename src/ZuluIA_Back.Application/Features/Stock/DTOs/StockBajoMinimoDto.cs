namespace ZuluIA_Back.Application.Features.Stock.DTOs;

public class StockBajoMinimoDto
{
    public long ItemId { get; set; }
    public string ItemCodigo { get; set; } = string.Empty;
    public string ItemDescripcion { get; set; } = string.Empty;
    public long DepositoId { get; set; }
    public string DepositoDescripcion { get; set; } = string.Empty;
    public decimal CantidadActual { get; set; }
    public decimal StockMinimo { get; set; }
    public decimal Diferencia { get; set; }
}