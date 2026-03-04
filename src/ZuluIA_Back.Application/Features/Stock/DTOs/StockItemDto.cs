namespace ZuluIA_Back.Application.Features.Stock.DTOs;

public class StockItemDto
{
    public long Id { get; set; }
    public long ItemId { get; set; }
    public string ItemCodigo { get; set; } = string.Empty;
    public string ItemDescripcion { get; set; } = string.Empty;
    public long DepositoId { get; set; }
    public string DepositoDescripcion { get; set; } = string.Empty;
    public bool DepositoEsDefault { get; set; }
    public decimal Cantidad { get; set; }
    public decimal StockMinimo { get; set; }
    public decimal? StockMaximo { get; set; }
    public bool BajoMinimo { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}