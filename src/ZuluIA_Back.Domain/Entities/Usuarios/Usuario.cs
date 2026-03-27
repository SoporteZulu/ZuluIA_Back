using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Domain.Entities.Usuarios;

public class Usuario : AuditableEntity
{
    public Guid? SupabaseUserId { get; private set; }
    public string UserName { get; private set; } = string.Empty;
    public string? NombreCompleto { get; private set; }
    public string? Email { get; private set; }
    public string? PasswordHash { get; private set; }
    public bool Activo { get; private set; } = true;
    public long? ArqueoActual { get; private set; }

    private readonly List<UsuarioSucursal> _sucursales = [];
    public IReadOnlyCollection<UsuarioSucursal> Sucursales => _sucursales.AsReadOnly();

    private Usuario() { }

    public static Usuario Crear(
        string userName,
        string? nombreCompleto,
        string? email,
        Guid? supabaseUserId,
        long? userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userName);

        var usuario = new Usuario
        {
            UserName       = userName.Trim().ToLowerInvariant(),
            NombreCompleto = nombreCompleto?.Trim(),
            Email          = email?.Trim().ToLowerInvariant(),
            SupabaseUserId = supabaseUserId,
            Activo         = true
        };

        usuario.SetCreated(userId);
        return usuario;
    }

    public void Actualizar(
        string? nombreCompleto,
        string? email,
        long? userId)
    {
        NombreCompleto = nombreCompleto?.Trim();
        Email          = email?.Trim().ToLowerInvariant();
        SetUpdated(userId);
    }

    public void EstablecerPasswordHash(string passwordHash, long? userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);

        PasswordHash = passwordHash.Trim();
        SetUpdated(userId);
    }

    public void AsignarSucursal(long sucursalId)
    {
        if (_sucursales.Any(x => x.SucursalId == sucursalId))
            return;

        _sucursales.Add(UsuarioSucursal.Crear(Id, sucursalId));
    }

    public void RemoverSucursal(long sucursalId)
    {
        var rel = _sucursales.FirstOrDefault(x => x.SucursalId == sucursalId);
        if (rel is not null)
            _sucursales.Remove(rel);
    }

    public void SetArqueoActual(long? arqueoId) => ArqueoActual = arqueoId;

    public void Desactivar(long? userId)
    {
        Activo = false;
        SetDeleted();
        SetUpdated(userId);
    }

    public void Activar(long? userId)
    {
        Activo = true;
        SetUpdated(userId);
    }
}