using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Usuarios;

/// <summary>
/// Permiso de acceso de un usuario a un ítem del menú.
/// </summary>
public class MenuUsuario : BaseEntity
{
    public long MenuId { get; private set; }
    public long UsuarioId { get; private set; }

    private MenuUsuario() { }

    public static MenuUsuario Crear(long menuId, long usuarioId) =>
        new() { MenuId = menuId, UsuarioId = usuarioId };
}