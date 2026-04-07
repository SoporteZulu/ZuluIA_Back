namespace ZuluIA_Back.Application.Features.Terceros.DTOs;

public class TerceroTransporteDto
{
    public long Id { get; set; }
    public long TerceroId { get; set; }
    public long? TransportistaId { get; set; }
    public string? TransportistaNombre { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Servicio { get; set; }
    public string? Zona { get; set; }
    public string? Frecuencia { get; set; }
    public string? Observacion { get; set; }
    public bool Activo { get; set; }
    public bool Principal { get; set; }
    public int Orden { get; set; }
}
