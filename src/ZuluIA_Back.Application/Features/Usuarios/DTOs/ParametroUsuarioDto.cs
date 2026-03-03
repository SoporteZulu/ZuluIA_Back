namespace ZuluIA_Back.Application.Features.Usuarios.DTOs;

public class ParametroUsuarioDto
{
    public long Id { get; set; }
    public long UsuarioId { get; set; }
    public string Clave { get; set; } = string.Empty;
    public string? Valor { get; set; }
}