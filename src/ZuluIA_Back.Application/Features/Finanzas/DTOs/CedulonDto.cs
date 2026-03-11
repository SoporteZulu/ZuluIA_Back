namespace ZuluIA_Back.Application.Features.Finanzas.DTOs;

public class CedulonDto
{
    public long Id { get; set; }
    public long TerceroId { get; set; }
    public string TerceroRazonSocial { get; set; } = string.Empty;
    public long SucursalId { get; set; }
    public long? PlanPagoId { get; set; }
    public string NroCedulon { get; set; } = string.Empty;
    public DateOnly FechaEmision { get; set; }
    public DateOnly FechaVencimiento { get; set; }
    public decimal Importe { get; set; }
    public decimal ImportePagado { get; set; }
    public decimal SaldoPendiente { get; set; }
    public string Estado { get; set; } = string.Empty;
    public bool Vencido { get; set; }
}