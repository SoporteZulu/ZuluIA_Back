namespace ZuluIA_Back.Application.Features.Comprobantes.DTOs;

public class ImputacionDto
{
    public long Id { get; set; }
    public long ComprobanteOrigenId { get; set; }
    public string NumeroOrigen { get; set; } = string.Empty;
    public long? TipoComprobanteOrigenId { get; set; }
    public string TipoComprobanteOrigen { get; set; } = string.Empty;
    public long ComprobanteDestinoId { get; set; }
    public string NumeroDestino { get; set; } = string.Empty;
    public long? TipoComprobanteDestinoId { get; set; }
    public string TipoComprobanteDestino { get; set; } = string.Empty;
    public decimal Importe { get; set; }
    public DateOnly Fecha { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public bool Anulada { get; set; }
    public DateOnly? FechaDesimputacion { get; set; }
    public string? MotivoDesimputacion { get; set; }
    public DateTimeOffset? DesimputadaAt { get; set; }
    public string RolComprobante { get; set; } = string.Empty;
}