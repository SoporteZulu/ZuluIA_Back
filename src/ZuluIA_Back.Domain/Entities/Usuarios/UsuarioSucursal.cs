using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Usuarios;

public class UsuarioSucursal : BaseEntity
{
    public long UsuarioId { get; private set; }
    public long SucursalId { get; private set; }

    private UsuarioSucursal() { }

    internal static UsuarioSucursal Crear(long usuarioId, long sucursalId) =>
        new() { UsuarioId = usuarioId, SucursalId = sucursalId };
}