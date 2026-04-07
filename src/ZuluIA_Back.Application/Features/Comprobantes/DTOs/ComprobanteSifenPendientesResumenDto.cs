using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Application.Features.Comprobantes.DTOs;

public class ComprobanteSifenPendientesResumenDto
{
    public int Total { get; set; }
    public int Reintentables { get; set; }
    public int ConIdentificadores { get; set; }
    public int Conciliables { get; set; }
    public int SinEstadoSifen { get; set; }
    public List<ComprobanteSifenPendientesEstadoResumenDto> Estados { get; set; } = [];
    public List<ComprobanteSifenPendientesCodigoResumenDto> CodigosRespuesta { get; set; } = [];
    public List<ComprobanteSifenPendientesMensajeResumenDto> MensajesRespuesta { get; set; } = [];
}

public class ComprobanteSifenPendientesEstadoResumenDto
{
    public EstadoSifenParaguay? EstadoSifen { get; set; }
    public int Cantidad { get; set; }
}

public class ComprobanteSifenPendientesCodigoResumenDto
{
    public string CodigoRespuesta { get; set; } = string.Empty;
    public int Cantidad { get; set; }
}

public class ComprobanteSifenPendientesMensajeResumenDto
{
    public string MensajeRespuesta { get; set; } = string.Empty;
    public int Cantidad { get; set; }
}