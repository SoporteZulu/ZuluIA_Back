using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Application.Features.Comprobantes.DTOs;

public class ComprobanteSifenHistorialDto
{
    public long Id { get; set; }
    public long ComprobanteId { get; set; }
    public long? UsuarioId { get; set; }
    public EstadoSifenParaguay EstadoSifen { get; set; }
    public bool Aceptado { get; set; }
    public string? EstadoRespuesta { get; set; }
    public string? CodigoRespuesta { get; set; }
    public string? MensajeRespuesta { get; set; }
    public string? TrackingId { get; set; }
    public string? Cdc { get; set; }
    public string? NumeroLote { get; set; }
    public DateTime FechaHora { get; set; }
    public DateTimeOffset? FechaRespuesta { get; set; }
    public string? Detalle { get; set; }
    public string? RespuestaCruda { get; set; }
}