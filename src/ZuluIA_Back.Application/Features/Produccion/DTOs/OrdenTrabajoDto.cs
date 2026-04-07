namespace ZuluIA_Back.Application.Features.Produccion.DTOs;

public class OrdenTrabajoDto
{
    public long Id { get; set; }
    public long SucursalId { get; set; }
    public long FormulaId { get; set; }
    public string FormulaCodigo { get; set; } = string.Empty;
    public string FormulaDescripcion { get; set; } = string.Empty;
    public long DepositoOrigenId { get; set; }
    public string DepositoOrigenDescripcion { get; set; } = string.Empty;
    public long DepositoDestinoId { get; set; }
    public string DepositoDestinoDescripcion { get; set; } = string.Empty;
    public DateOnly Fecha { get; set; }
    public DateOnly? FechaFinPrevista { get; set; }
    public DateOnly? FechaFinReal { get; set; }
    public decimal Cantidad { get; set; }
    public decimal? CantidadProducida { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string? Observacion { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}