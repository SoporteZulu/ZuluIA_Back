namespace ZuluIA_Back.Application.Features.Terceros.DTOs;

public class TerceroContactoDto
{
    public long Id { get; set; }
    public long TerceroId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Cargo { get; set; }
    public string? Email { get; set; }
    public string? Telefono { get; set; }
    public string? Sector { get; set; }
    public bool Principal { get; set; }
    public int Orden { get; set; }
}
