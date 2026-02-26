using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Application.Features.Comprobantes.DTOs;

public class ComprobanteDto
{
    public long Id { get; set; }
    public long SucursalId { get; set; }
    public long? PuntoFacturacionId { get; set; }
    public long TipoComprobanteId { get; set; }
    public short Prefijo { get; set; }
    public long Numero { get; set; }
    public string NroFormateado { get; set; } = string.Empty;
    public DateOnly Fecha { get; set; }
    public DateOnly? FechaVencimiento { get; set; }
    public long TerceroId { get; set; }
    public long MonedaId { get; set; }
    public decimal Cotizacion { get; set; }
    public decimal Subtotal { get; set; }
    public decimal DescuentoImporte { get; set; }
    public decimal NetoGravado { get; set; }
    public decimal NetoNoGravado { get; set; }
    public decimal IvaRi { get; set; }
    public decimal Total { get; set; }
    public decimal Saldo { get; set; }
    public string? Cae { get; set; }
    public DateOnly? FechaVtoCae { get; set; }
    public EstadoComprobante Estado { get; set; }
    public string? Observacion { get; set; }
    public List<ComprobanteItemDto> Items { get; set; } = [];
    public DateTimeOffset CreatedAt { get; set; }
}