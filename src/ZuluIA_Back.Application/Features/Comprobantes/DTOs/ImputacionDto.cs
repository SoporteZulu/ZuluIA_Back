namespace ZuluIA_Back.Application.Features.Comprobantes.DTOs;

public class ImputacionDto
{
    public long Id { get; set; }
    public long ComprobanteOrigenId { get; set; }
    public string NumeroOrigen { get; set; } = string.Empty;
    public long ComprobanteDestinoId { get; set; }
    public string NumeroDestino { get; set; } = string.Empty;
    public decimal Importe { get; set; }
    public DateOnly Fecha { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}