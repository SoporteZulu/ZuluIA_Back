namespace ZuluIA_Back.Application.Features.Comprobantes.DTOs;

/// <summary>
/// DTO simplificado de comprobante pendiente para selección en cobros
/// </summary>
public class ComprobantePendienteCobroDto
{
    public long Id { get; set; }
    public string TipoComprobante { get; set; } = string.Empty;
    public string TipoComprobanteCodigo { get; set; } = string.Empty;
    public short Prefijo { get; set; }
    public long Numero { get; set; }
    public string NumeroFormateado { get; set; } = string.Empty;
    public DateOnly FechaEmision { get; set; }
    public DateOnly? FechaVencimiento { get; set; }
    public int? DiasMora { get; set; }
    public bool EstaVencido { get; set; }
    public long MonedaId { get; set; }
    public string MonedaSimbolo { get; set; } = string.Empty;
    public decimal Cotizacion { get; set; }
    public decimal ImporteTotal { get; set; }
    public decimal ImporteCobrado { get; set; }
    public decimal SaldoPendiente { get; set; }
    public string? Observacion { get; set; }
    
    // Para calcular importe a imputar en moneda del cobro
    public decimal ImporteAImputar { get; set; }
}
