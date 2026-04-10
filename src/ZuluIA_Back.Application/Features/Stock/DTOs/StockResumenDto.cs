namespace ZuluIA_Back.Application.Features.Stock.DTOs;

public class StockResumenDto
{
    public long ItemId { get; set; }
    public string ItemCodigo { get; set; } = string.Empty;
    public string ItemDescripcion { get; set; } = string.Empty;
    public decimal StockTotal { get; set; }
    public decimal TotalStock
    {
        get => StockTotal;
        set => StockTotal = value;
    }
    public decimal StockMinimo { get; set; }
    public decimal? StockMaximo { get; set; }
    public bool BajoMinimo { get; set; }
    public IReadOnlyList<StockPorDepositoDto> PorDeposito { get; set; } = [];
    public IReadOnlyList<StockPorDepositoDto> Depositos
    {
        get => PorDeposito;
        set => PorDeposito = value;
    }
}

public class StockPorDepositoDto
{
    public long Id { get; set; }
    public long ItemId { get; set; }
    public long DepositoId { get; set; }
    public string DepositoDescripcion { get; set; } = string.Empty;
    public bool EsDefault { get; set; }
    public decimal Cantidad { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}