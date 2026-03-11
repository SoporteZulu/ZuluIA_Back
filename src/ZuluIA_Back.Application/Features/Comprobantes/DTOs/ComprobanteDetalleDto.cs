namespace ZuluIA_Back.Application.Features.Comprobantes.DTOs;

public class ComprobanteDetalleDto
{
    public long Id { get; set; }
    public long SucursalId { get; set; }
    public string SucursalRazonSocial { get; set; } = string.Empty;
    public long? PuntoFacturacionId { get; set; }
    public long TipoComprobanteId { get; set; }
    public string TipoComprobanteDescripcion { get; set; } = string.Empty;
    public string? TipoComprobanteCodigo { get; set; }
    public short Prefijo { get; set; }
    public long Numero { get; set; }
    public string NumeroFormateado { get; set; } = string.Empty;
    public DateOnly Fecha { get; set; }
    public DateOnly? FechaVencimiento { get; set; }
    public long TerceroId { get; set; }
    public string TerceroRazonSocial { get; set; } = string.Empty;
    public string TerceroCuit { get; set; } = string.Empty;
    public string TerceroCondicionIva { get; set; } = string.Empty;
    public long MonedaId { get; set; }
    public string MonedaSimbolo { get; set; } = string.Empty;
    public decimal Cotizacion { get; set; }
    public decimal Subtotal { get; set; }
    public decimal DescuentoImporte { get; set; }
    public decimal NetoGravado { get; set; }
    public decimal NetoNoGravado { get; set; }
    public decimal IvaRi { get; set; }
    public decimal IvaRni { get; set; }
    public decimal Percepciones { get; set; }
    public decimal Retenciones { get; set; }
    public decimal Total { get; set; }
    public decimal Saldo { get; set; }
    public string? Cae { get; set; }
    public DateOnly? FechaVtoCae { get; set; }
    public string? QrData { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string? Observacion { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public IReadOnlyList<ComprobanteItemDto> Items { get; set; } = [];
    public IReadOnlyList<ImputacionDto> Imputaciones { get; set; } = [];
}