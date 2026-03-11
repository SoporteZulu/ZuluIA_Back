using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Usuarios;

/// <summary>
/// Define un permiso/función del sistema que puede asignarse a usuarios.
/// </summary>
public class Seguridad : BaseEntity
{
    public string Identificador { get; private set; } = string.Empty;
    public string? Descripcion { get; private set; }
    public bool AplicaSeguridadPorUsuario { get; private set; } = true;

    private Seguridad() { }

    public static Seguridad Crear(
        string identificador,
        string? descripcion,
        bool aplicaSeguridadPorUsuario = true)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(identificador);

        return new Seguridad
        {
            Identificador               = identificador.Trim().ToUpperInvariant(),
            Descripcion                 = descripcion?.Trim(),
            AplicaSeguridadPorUsuario   = aplicaSeguridadPorUsuario
        };
    }
}