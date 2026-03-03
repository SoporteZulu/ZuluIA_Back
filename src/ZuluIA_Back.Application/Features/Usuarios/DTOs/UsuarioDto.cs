namespace ZuluIA_Back.Application.Features.Usuarios.DTOs;

public class UsuarioDto
{
    public long Id { get; set; }
    public Guid? SupabaseUserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? NombreCompleto { get; set; }
    public string? Email { get; set; }
    public bool Activo { get; set; }
    public long? ArqueoActual { get; set; }
    public IReadOnlyList<long> SucursalIds { get; set; } = [];
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}