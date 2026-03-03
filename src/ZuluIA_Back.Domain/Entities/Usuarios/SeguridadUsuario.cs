using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Usuarios;

/// <summary>
/// Valor de un permiso de seguridad para un usuario específico.
/// </summary>
public class SeguridadUsuario : BaseEntity
{
    public long SeguridadId { get; private set; }
    public long UsuarioId { get; private set; }
    public bool Valor { get; private set; }

    private SeguridadUsuario() { }

    public static SeguridadUsuario Crear(long seguridadId, long usuarioId, bool valor) =>
        new() { SeguridadId = seguridadId, UsuarioId = usuarioId, Valor = valor };

    public void SetValor(bool valor) => Valor = valor;
}