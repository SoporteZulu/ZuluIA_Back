namespace ZuluIA_Back.Application.Features.Stock.DTOs;

public class StockBajoMinimoDto
{
    public long ItemId { get; set; }
    public string ItemCodigo { get; set; } = string.Empty;
    public string Codigo
    {
        get => ItemCodigo;
        set => ItemCodigo = value;
    }
    public string ItemDescripcion { get; set; } = string.Empty;
    public string Descripcion
    {
        get => ItemDescripcion;
        set => ItemDescripcion = value;
    }
    public long DepositoId { get; set; }
    public string DepositoDescripcion { get; set; } = string.Empty;
    public decimal CantidadActual { get; set; }
    public decimal StockActual
    {
        get => CantidadActual;
        set => CantidadActual = value;
    }
    public decimal StockMinimo { get; set; }
    public decimal Diferencia { get; set; }
}