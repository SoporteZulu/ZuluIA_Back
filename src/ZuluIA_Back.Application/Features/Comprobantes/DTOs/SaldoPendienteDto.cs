namespace ZuluIA_Back.Application.Features.Comprobantes.DTOs;

public class SaldoPendienteDto
{
    public long ComprobanteId { get; set; }
    public string NumeroFormateado { get; set; } = string.Empty;
    public string TipoComprobante { get; set; } = string.Empty;
    public DateOnly Fecha { get; set; }
    public DateOnly? FechaVencimiento { get; set; }
    public decimal Total { get; set; }
    public decimal Saldo { get; set; }
    public bool Vencido { get; set; }
}