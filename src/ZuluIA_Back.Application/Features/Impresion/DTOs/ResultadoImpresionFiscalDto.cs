namespace ZuluIA_Back.Application.Features.Impresion.DTOs;

public class ResultadoImpresionFiscalDto
{
    public string Marca { get; set; } = string.Empty;
    public long ComprobanteId { get; set; }
    public string NumeroComprobante { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
    public string PayloadFiscal { get; set; } = string.Empty;
}
