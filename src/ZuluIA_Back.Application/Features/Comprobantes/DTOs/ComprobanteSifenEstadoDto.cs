using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Application.Features.Comprobantes.DTOs;

public class ComprobanteSifenEstadoDto
{
    public long ComprobanteId { get; set; }
    public EstadoComprobante EstadoComprobante { get; set; }
    public EstadoSifenParaguay? EstadoSifen { get; set; }
    public string? SifenCodigoRespuesta { get; set; }
    public string? SifenMensajeRespuesta { get; set; }
    public string? SifenTrackingId { get; set; }
    public string? SifenCdc { get; set; }
    public string? SifenNumeroLote { get; set; }
    public DateTimeOffset? SifenFechaRespuesta { get; set; }
    public long? TimbradoId { get; set; }
    public string? NroTimbrado { get; set; }
    public bool FueAceptado { get; set; }
    public bool TieneIdentificadores { get; set; }
    public bool PuedeReintentar { get; set; }
    public bool PuedeConciliar { get; set; }
}