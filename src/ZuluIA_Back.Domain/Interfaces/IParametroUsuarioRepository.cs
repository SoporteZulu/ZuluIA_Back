using ZuluIA_Back.Domain.Entities.Usuarios;

namespace ZuluIA_Back.Domain.Interfaces;

public interface IParametroUsuarioRepository : IRepository<ParametroUsuario>
{
    Task<ParametroUsuario?> GetByClaveAsync(
        long usuarioId,
        string clave,
        CancellationToken ct = default);

    Task<IReadOnlyList<ParametroUsuario>> GetByUsuarioAsync(
        long usuarioId,
        CancellationToken ct = default);

    Task UpsertAsync(
        long usuarioId,
        string clave,
        string? valor,
        CancellationToken ct = default);
}