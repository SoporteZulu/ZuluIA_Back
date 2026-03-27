namespace ZuluIA_Back.Application.Features.Terceros.DTOs;

public class TerceroVentanaCobranzaDto
{
    public long Id { get; set; }
    public long TerceroId { get; set; }
    public string Dia { get; set; } = string.Empty;
    public string? Franja { get; set; }
    public string? Canal { get; set; }
    public string? Responsable { get; set; }
    public bool Principal { get; set; }
    public int Orden { get; set; }
}
