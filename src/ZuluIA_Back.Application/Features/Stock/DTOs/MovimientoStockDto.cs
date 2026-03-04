namespace ZuluIA_Back.Application.Features.Stock.DTOs;

public class MovimientoStockDto
{
    public long Id { get; set; }
    public long ItemId { get; set; }
    public string ItemCodigo { get; set; } = string.Empty;
    public string ItemDescripcion { get; set; } = string.Empty;
    public long DepositoId { get; set; }
    public string DepositoDescripcion { get; set; } = string.Empty;
    public DateTimeOffset Fecha { get; set; }
    public string TipoMovimiento { get; set; } = string.Empty;
    public decimal Cantidad { get; set; }
    public decimal SaldoResultante { get; set; }
    public string? OrigenTabla { get; set; }
    public long? OrigenId { get; set; }
    public string? Observacion { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public long? CreatedBy { get; set; }
}