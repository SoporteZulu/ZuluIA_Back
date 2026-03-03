using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Domain.Services;

/// <summary>
/// Servicio de dominio que centraliza la lógica de evaluación de permisos.
/// </summary>
public class PermisoService(ISeguridadRepository seguridadRepo)
{
    /// <summary>
    /// Verifica si un usuario tiene un permiso.
    /// Si el permiso no aplica seguridad por usuario, siempre retorna true.
    /// </summary>
    public async Task<bool> TienePermisoAsync(
        long usuarioId,
        string identificador,
        CancellationToken ct = default)
    {
        var seguridad = await seguridadRepo.GetByIdentificadorAsync(identificador, ct);

        if (seguridad is null)
            return false;

        // Si no aplica seguridad por usuario, todos tienen acceso
        if (!seguridad.AplicaSeguridadPorUsuario)
            return true;

        return await seguridadRepo.TienePermisoAsync(usuarioId, identificador, ct);
    }

    /// <summary>
    /// Retorna el mapa completo de permisos de un usuario.
    /// </summary>
    public async Task<Dictionary<string, bool>> GetPermisosAsync(
        long usuarioId,
        CancellationToken ct = default) =>
        await seguridadRepo.GetPermisosUsuarioAsync(usuarioId, ct);
}