namespace ZuluIA_Back.Application.Features.Terceros.DTOs;

public class TerceroUsuarioClienteDto
{
    public long UsuarioId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? NombreCompleto { get; set; }
    public string? Email { get; set; }
    public bool Activo { get; set; }
    public bool TienePasswordConfigurada { get; set; }
    public long? UsuarioGrupoId { get; set; }
    public string? UsuarioGrupoUserName { get; set; }
    public TerceroUsuarioClienteParametrosBasicosDto ParametrosBasicos { get; set; } = new();
    public IReadOnlyList<TerceroUsuarioClientePermisoDto> PermisosBasicos { get; set; } = [];
}
