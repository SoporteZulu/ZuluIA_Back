using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Application.Features.Comprobantes.DTOs;

public class ComprobanteListDto
{
    public long Id { get; set; }
    public long SucursalId { get; set; }
    public long TipoComprobanteId { get; set; }
    public string TipoComprobanteDescripcion { get; set; } = string.Empty;
    public short Prefijo { get; set; }
    public long Numero { get; set; }
    public string NumeroFormateado { get; set; } = string.Empty;
    public DateOnly Fecha { get; set; }
    public DateOnly? FechaVencimiento { get; set; }
    public long TerceroId { get; set; }
    public string TerceroRazonSocial { get; set; } = string.Empty;
    public long MonedaId { get; set; }
    public string MonedaSimbolo { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public decimal Saldo { get; set; }
    public EstadoComprobante Estado { get; set; }
    public string? Cae { get; set; }
    public bool TieneCae => !string.IsNullOrWhiteSpace(Cae);
}
