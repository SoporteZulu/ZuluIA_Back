namespace ZuluIA_Back.Application.Features.Extras.DTOs;

public class TransportistaDto
{
    public long Id { get; set; }
    public long TerceroId { get; set; }
    public string TerceroRazonSocial { get; set; } = string.Empty;
    public string TerceroCuit { get; set; } = string.Empty;
    public string? NroCuitTransportista { get; set; }
    public string? DomicilioPartida { get; set; }
    public string? Patente { get; set; }
    public string? MarcaVehiculo { get; set; }
    public bool Activo { get; set; }
}