namespace ZuluIA_Back.Application.Features.Usuarios.DTOs;

public class UsuarioListDto
{
    public long Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? NombreCompleto { get; set; }
    public string? Email { get; set; }
    public bool Activo { get; set; }
}