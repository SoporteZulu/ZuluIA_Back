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
    public long? TimbradoId { get; set; }
    public string? NroTimbrado { get; set; }
    public EstadoSifenParaguay? EstadoSifen { get; set; }
    public string? SifenCodigoRespuesta { get; set; }
    public string? SifenMensajeRespuesta { get; set; }
    public string? SifenTrackingId { get; set; }
    public string? SifenCdc { get; set; }
    public string? SifenNumeroLote { get; set; }
    public DateTimeOffset? SifenFechaRespuesta { get; set; }
    public bool TieneIdentificadoresSifen { get; set; }
    public bool PuedeReintentarSifen { get; set; }
    public bool PuedeConciliarSifen { get; set; }
    public string? Cae { get; set; }
    public string? Caea { get; set; }
    public DateOnly? FechaVtoCae { get; set; }
    public EstadoAfipWsfe EstadoAfip { get; set; }
    public string? UltimoErrorAfip { get; set; }
    public DateTimeOffset? FechaUltimaConsultaAfip { get; set; }
    public EstadoComprobante Estado { get; set; }
    public string? Observacion { get; set; }
    public List<ComprobanteItemDto> Items { get; set; } = [];
    public List<ComprobanteImpuestoDto> Impuestos { get; set; } = [];
    public List<ComprobanteTributoDto> Tributos { get; set; } = [];
    public DateTimeOffset CreatedAt { get; set; }
}

public class ComprobanteImpuestoDto
{
    public long AlicuotaIvaId { get; set; }
    public decimal PorcentajeIva { get; set; }
    public decimal BaseImponible { get; set; }
    public decimal ImporteIva { get; set; }
}

public class ComprobanteTributoDto
{
    public long? ImpuestoId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public decimal BaseImponible { get; set; }
    public decimal Alicuota { get; set; }
    public decimal Importe { get; set; }
    public int Orden { get; set; }
}