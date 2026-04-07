using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Application.Features.Comprobantes.DTOs;

public class ConciliacionSifenParaguayPreviewDto
{
    public int MaxItems { get; set; }
    public long? SucursalId { get; set; }
    public int Encontrados { get; set; }
    public int TotalElegibles { get; set; }
    public bool HayMasResultados { get; set; }
    public List<ConciliacionSifenParaguayPreviewItemDto> Items { get; set; } = [];
    public List<ConciliacionSifenParaguayPreviewEstadoResumenDto> Estados { get; set; } = [];
    public List<ConciliacionSifenParaguayPreviewCodigoResumenDto> CodigosRespuesta { get; set; } = [];
    public List<ConciliacionSifenParaguayPreviewMensajeResumenDto> MensajesRespuesta { get; set; } = [];
}

public class ConciliacionSifenParaguayPreviewEstadoResumenDto
{
    public EstadoSifenParaguay? EstadoSifen { get; set; }
    public int Cantidad { get; set; }
}

public class ConciliacionSifenParaguayPreviewCodigoResumenDto
{
    public string CodigoRespuesta { get; set; } = string.Empty;
    public int Cantidad { get; set; }
}

public class ConciliacionSifenParaguayPreviewMensajeResumenDto
{
    public string MensajeRespuesta { get; set; } = string.Empty;
    public int Cantidad { get; set; }
}

public class ConciliacionSifenParaguayPreviewItemDto
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
    public bool PuedeReintentar { get; set; }
}