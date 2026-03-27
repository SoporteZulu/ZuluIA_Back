using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Usuarios;

/// <summary>
/// Relación de pertenencia entre usuarios (miembro / grupo).
/// Migrado desde VB6: clsUsuarioXUsuario / SEG_USUARIOXUSUARIO.
/// Permite definir jerarquías, sustituciones y agrupaciones de usuarios.
/// </summary>
public class UsuarioXUsuario : BaseEntity
{
    public long UsuarioMiembroId { get; private set; }  // usr_id_miembro
    public long UsuarioGrupoId   { get; private set; }  // usr_id_grupo

    private UsuarioXUsuario() { }

    public static UsuarioXUsuario Crear(long usuarioMiembroId, long usuarioGrupoId)
    {
        if (usuarioMiembroId <= 0) throw new ArgumentException("UsuarioMiembroId es requerido.");
        if (usuarioGrupoId   <= 0) throw new ArgumentException("UsuarioGrupoId es requerido.");
        if (usuarioMiembroId == usuarioGrupoId) throw new ArgumentException("Un usuario no puede pertenecer a sí mismo.");
        return new UsuarioXUsuario
        {
            UsuarioMiembroId = usuarioMiembroId,
            UsuarioGrupoId   = usuarioGrupoId
        };
    }
}
