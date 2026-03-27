namespace ZuluIA_Back.Application.Features.Facturacion.DTOs;

public class CartaPorteDto
{
    public long Id { get; set; }
    public long? ComprobanteId { get; set; }
    public long? OrdenCargaId { get; set; }
    public long? TransportistaId { get; set; }
    public string? NroCtg { get; set; }
    public string CuitRemitente { get; set; } = string.Empty;
    public string CuitDestinatario { get; set; } = string.Empty;
    public string? CuitTransportista { get; set; }
    public DateOnly FechaEmision { get; set; }
    public DateOnly? FechaSolicitudCtg { get; set; }
    public int IntentosCtg { get; set; }
    public string? UltimoErrorCtg { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string? Observacion { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}