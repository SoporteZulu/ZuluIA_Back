using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Application.Features.Comprobantes.DTOs;

public class ReintentoSifenParaguayPreviewDto
{
    public int MaxItems { get; set; }
    public long? SucursalId { get; set; }
    public int Encontrados { get; set; }
    public int TotalElegibles { get; set; }
    public bool HayMasResultados { get; set; }
    public List<ReintentoSifenParaguayPreviewItemDto> Items { get; set; } = [];
    public List<ReintentoSifenParaguayPreviewEstadoResumenDto> Estados { get; set; } = [];
    public List<ReintentoSifenParaguayPreviewCodigoResumenDto> CodigosRespuesta { get; set; } = [];
    public List<ReintentoSifenParaguayPreviewMensajeResumenDto> MensajesRespuesta { get; set; } = [];
}

public class ReintentoSifenParaguayPreviewEstadoResumenDto
{
    public EstadoSifenParaguay? EstadoSifen { get; set; }
    public int Cantidad { get; set; }
}

public class ReintentoSifenParaguayPreviewCodigoResumenDto
{
    public string CodigoRespuesta { get; set; } = string.Empty;
    public int Cantidad { get; set; }
}

public class ReintentoSifenParaguayPreviewMensajeResumenDto
{
    public string MensajeRespuesta { get; set; } = string.Empty;
    public int Cantidad { get; set; }
}

public class ReintentoSifenParaguayPreviewItemDto
{
    public long ComprobanteId { get; set; }
    public long SucursalId { get; set; }
    public DateOnly Fecha { get; set; }
    public EstadoSifenParaguay? EstadoSifen { get; set; }
    public string? CodigoRespuesta { get; set; }
    public string? MensajeRespuesta { get; set; }
    public string? TrackingId { get; set; }
    public string? Cdc { get; set; }
    public string? NumeroLote { get; set; }
    public DateTimeOffset? FechaRespuesta { get; set; }
}