using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Domain.Entities.Auditoria;

public class HistorialSifenComprobante : BaseEntity
{
    public long ComprobanteId { get; private set; }
    public long? UsuarioId { get; private set; }
    public EstadoSifenParaguay EstadoSifen { get; private set; }
    public bool Aceptado { get; private set; }
    public string? EstadoRespuesta { get; private set; }
    public string? CodigoRespuesta { get; private set; }
    public string? MensajeRespuesta { get; private set; }
    public string? TrackingId { get; private set; }
    public string? Cdc { get; private set; }
    public string? NumeroLote { get; private set; }
    public DateTime FechaHora { get; private set; }
    public DateTimeOffset? FechaRespuesta { get; private set; }
    public string? Detalle { get; private set; }
    public string? RespuestaCruda { get; private set; }

    private HistorialSifenComprobante() { }

    public static HistorialSifenComprobante Registrar(
        long comprobanteId,
        long? usuarioId,
        EstadoSifenParaguay estadoSifen,
        bool aceptado,
        string? estadoRespuesta,
        string? codigoRespuesta,
        string? mensajeRespuesta,
        string? trackingId,
        string? cdc,
        string? numeroLote,
        DateTimeOffset? fechaRespuesta,
        string? detalle,
        string? respuestaCruda)
    {
        return new HistorialSifenComprobante
        {
            ComprobanteId = comprobanteId,
            UsuarioId = usuarioId,
            EstadoSifen = estadoSifen,
            Aceptado = aceptado,
            EstadoRespuesta = string.IsNullOrWhiteSpace(estadoRespuesta) ? null : estadoRespuesta.Trim(),
            CodigoRespuesta = string.IsNullOrWhiteSpace(codigoRespuesta) ? null : codigoRespuesta.Trim(),
            MensajeRespuesta = string.IsNullOrWhiteSpace(mensajeRespuesta) ? null : mensajeRespuesta.Trim(),
            TrackingId = string.IsNullOrWhiteSpace(trackingId) ? null : trackingId.Trim(),
            Cdc = string.IsNullOrWhiteSpace(cdc) ? null : cdc.Trim(),
            NumeroLote = string.IsNullOrWhiteSpace(numeroLote) ? null : numeroLote.Trim(),
            FechaHora = DateTime.UtcNow,
            FechaRespuesta = fechaRespuesta,
            Detalle = string.IsNullOrWhiteSpace(detalle) ? null : detalle.Trim(),
            RespuestaCruda = string.IsNullOrWhiteSpace(respuestaCruda) ? null : respuestaCruda.Trim()
        };
    }
}