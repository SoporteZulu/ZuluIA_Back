namespace ZuluIA_Back.Application.Features.Facturacion.DTOs;

public class ProximoNumeroDto
{
    public long PuntoFacturacionId { get; set; }
    public long TipoComprobanteId { get; set; }
    public short Prefijo { get; set; }
    public long ProximoNumero { get; set; }
    public string NumeroFormateado { get; set; } = string.Empty;
}