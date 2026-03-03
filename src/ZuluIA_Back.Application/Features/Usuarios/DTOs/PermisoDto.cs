namespace ZuluIA_Back.Application.Features.Usuarios.DTOs;

public class PermisoDto
{
    public long SeguridadId { get; set; }
    public string Identificador { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public bool Valor { get; set; }
    public bool AplicaSeguridadPorUsuario { get; set; }
}