using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Application.Features.Comprobantes.DTOs;

public class ComprobanteListDto
{
    public long Id { get; set; }
    public long SucursalId { get; set; }
    public string? SucursalCodigo { get; set; }
    public long TipoComprobanteId { get; set; }
    public string TipoComprobanteDescripcion { get; set; } = string.Empty;
    public short Prefijo { get; set; }
    public long Numero { get; set; }
    public string NumeroFormateado { get; set; } = string.Empty;
    public string NroComprobante => NumeroFormateado;
    public DateOnly Fecha { get; set; }
    public DateOnly? FechaVencimiento { get; set; }
    public DateOnly? FechaVto => FechaVencimiento;
    public long TerceroId { get; set; }
    public string TerceroRazonSocial { get; set; } = string.Empty;
    public string? TerceroLegajo { get; set; }
    public string? TerceroLegajoSucursal { get; set; }
    public long MonedaId { get; set; }
    public string MonedaSimbolo { get; set; } = string.Empty;
    public long? DepositoOrigenId { get; set; }
    public string? DepositoDescripcion { get; set; }
    public string? CotNumero { get; set; }
    public DateOnly? CotFechaVigencia { get; set; }
    public EstadoLogisticoRemito? EstadoLogistico { get; set; }
    public bool EsValorizado { get; set; }
    public decimal NetoGravado { get; set; }
    public decimal NetoNoGravado { get; set; }
    public decimal IvaRi { get; set; }
    public decimal IvaRni { get; set; }
    public decimal Total { get; set; }
    public decimal Saldo { get; set; }
    public long? MotivoDebitoId { get; set; }
    public string? MotivoDebitoDescripcion { get; set; }
    public long? ComprobanteOrigenId { get; set; }
    public string? ComprobanteOrigenNumero { get; set; }
    public DateOnly? ComprobanteOrigenFecha { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string? Cae { get; set; }
    public DateOnly? FechaVtoCae { get; set; }
    public DateOnly? CaeFechaVto => FechaVtoCae;
    public string? QrData { get; set; }
    public string? Observacion { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public bool TieneCae => !string.IsNullOrWhiteSpace(Cae);
    public long? NotaDebitoId { get; set; }
    public string? NotaDebitoDescripcion { get; set; }
}
