using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Application.Features.Comprobantes.DTOs;

public class ComprobanteListDto
{
    public long Id { get; set; }
    public string NroFormateado { get; set; } = string.Empty;
    public string TipoDescripcion { get; set; } = string.Empty;
    public DateOnly Fecha { get; set; }
    public string RazonSocial { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public decimal Saldo { get; set; }
    public EstadoComprobante Estado { get; set; }
    public string? Cae { get; set; }
}