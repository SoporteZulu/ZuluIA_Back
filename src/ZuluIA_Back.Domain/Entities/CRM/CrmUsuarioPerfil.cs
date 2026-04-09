using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.CRM;

public class CrmUsuarioPerfil : AuditableEntity
{
    public long UsuarioId { get; private set; }
    public string Rol { get; private set; } = string.Empty;
    public string? Avatar { get; private set; }
    public bool Activo { get; private set; } = true;

    private CrmUsuarioPerfil() { }

    public static CrmUsuarioPerfil Crear(long usuarioId, string rol, string? avatar, long? userId)
    {
        if (usuarioId <= 0)
            throw new ArgumentException("El perfil CRM requiere un usuario válido.", nameof(usuarioId));

        var entity = new CrmUsuarioPerfil
        {
            UsuarioId = usuarioId,
            Rol = NormalizeRequired(rol, nameof(rol)),
            Avatar = NormalizeOptional(avatar),
            Activo = true
        };

        entity.SetCreated(userId);
        return entity;
    }

    public void Actualizar(string rol, string? avatar, bool activo, long? userId)
    {
        Rol = NormalizeRequired(rol, nameof(rol));
        Avatar = NormalizeOptional(avatar);
        Activo = activo;
        if (activo)
        {
            SetDeletedAt(null);
        }
        else
        {
            SetDeleted();
        }

        SetUpdated(userId);
    }

    private static string NormalizeRequired(string value, string paramName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, paramName);
        return value.Trim();
    }

    private static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
