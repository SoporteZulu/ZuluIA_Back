namespace ZuluIA_Back.Application.Features.Extras.DTOs;

public class BusquedaDto
{
    public long Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Modulo { get; set; } = string.Empty;
    public string FiltrosJson { get; set; } = "{}";
    public long? UsuarioId { get; set; }
    public bool EsGlobal { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}