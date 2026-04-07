namespace ZuluIA_Back.Application.Features.Facturacion.DTOs;

public class PreparacionSifenParaguayDto
{
    public long ComprobanteId { get; set; }
    public bool EsSucursalParaguay { get; set; }
    public bool IntegracionHabilitada { get; set; }
    public bool ListoParaEnviar { get; set; }
    public string Ambiente { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty;
    public string ModoTransporte { get; set; } = string.Empty;
    public List<string> Errores { get; set; } = [];
    public PreparacionSifenDocumentoDto Documento { get; set; } = new();
}

public class PreparacionSifenDocumentoDto
{
    public string RucEmisor { get; set; } = string.Empty;
    public string RazonSocialEmisor { get; set; } = string.Empty;
    public string? DireccionEmisor { get; set; }
    public string DocumentoReceptor { get; set; } = string.Empty;
    public string RazonSocialReceptor { get; set; } = string.Empty;
    public string? DireccionReceptor { get; set; }
    public string CodigoTipoComprobante { get; set; } = string.Empty;
    public string DescripcionTipoComprobante { get; set; } = string.Empty;
    public short? PuntoExpedicion { get; set; }
    public short Prefijo { get; set; }
    public long NumeroComprobante { get; set; }
    public DateOnly FechaEmision { get; set; }
    public decimal Total { get; set; }
    public decimal NetoGravado { get; set; }
    public decimal Iva { get; set; }
    public decimal NetoNoGravado { get; set; }
    public decimal Percepciones { get; set; }
    public int CantidadItems { get; set; }
    public long? TimbradoId { get; set; }
    public string? NroTimbrado { get; set; }
    public string? Observacion { get; set; }
}

public class ResultadoEnvioSifenParaguayDto
{
    public long ComprobanteId { get; set; }
    public bool Aceptado { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string? CodigoRespuesta { get; set; }
    public string? MensajeRespuesta { get; set; }
    public string? TrackingId { get; set; }
    public string? Cdc { get; set; }
    public string? NumeroLote { get; set; }
    public DateTimeOffset? FechaRespuesta { get; set; }
    public string? RespuestaCruda { get; set; }
}