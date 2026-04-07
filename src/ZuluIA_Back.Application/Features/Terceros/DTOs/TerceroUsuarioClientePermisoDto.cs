namespace ZuluIA_Back.Application.Features.Terceros.DTOs;

public class TerceroUsuarioClientePermisoDto
{
    public long SeguridadId { get; set; }
    public string Identificador { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public bool Valor { get; set; }
}
