using ZuluIA_Back.Domain.Entities.Usuarios;

namespace ZuluIA_Back.Domain.Interfaces;

public interface ISeguridadRepository : IRepository<Seguridad>
{
    /// <summary>
    /// Retorna todos los permisos con su valor para un usuario dado.
    /// </summary>
    Task<Dictionary<string, bool>> GetPermisosUsuarioAsync(
        long usuarioId,
        CancellationToken ct = default);

    /// <summary>
    /// Verifica si un usuario tiene un permiso específico habilitado.
    /// </summary>
    Task<bool> TienePermisoAsync(
        long usuarioId,
        string identificador,
        CancellationToken ct = default);

    Task<Seguridad?> GetByIdentificadorAsync(
        string identificador,
        CancellationToken ct = default);
}