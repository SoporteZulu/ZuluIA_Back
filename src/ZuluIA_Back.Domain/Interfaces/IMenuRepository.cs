using ZuluIA_Back.Domain.Entities.Usuarios;

namespace ZuluIA_Back.Domain.Interfaces;

public interface IMenuRepository : IRepository<MenuItem>
{
    /// <summary>
    /// Retorna el árbol de menú completo activo.
    /// </summary>
    Task<IReadOnlyList<MenuItem>> GetArbolCompletoAsync(CancellationToken ct = default);

    /// <summary>
    /// Retorna los ítems de menú a los que tiene acceso un usuario.
    /// </summary>
    Task<IReadOnlyList<MenuItem>> GetMenuUsuarioAsync(long usuarioId, CancellationToken ct = default);

    /// <summary>
    /// Retorna los IDs de menú asignados a un usuario.
    /// </summary>
    Task<IReadOnlyList<long>> GetMenuIdsUsuarioAsync(long usuarioId, CancellationToken ct = default);
}