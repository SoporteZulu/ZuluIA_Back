using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Usuarios;

namespace ZuluIA_Back.Domain.Interfaces;

public interface IUsuarioRepository : IRepository<Usuario>
{
    Task<Usuario?> GetByUserNameAsync(string userName, CancellationToken ct = default);
    Task<Usuario?> GetByUserNameOrEmailAsync(string login, CancellationToken ct = default);
    Task<Usuario?> GetBySupabaseIdAsync(Guid supabaseId, CancellationToken ct = default);
    Task<Usuario?> GetByIdConSucursalesAsync(long id, CancellationToken ct = default);
    Task<bool> ExisteUserNameAsync(string userName, long? excludeId = null, CancellationToken ct = default);
    Task<PagedResult<Usuario>> GetPagedAsync(int page, int pageSize, string? search, bool? soloActivos, CancellationToken ct = default);
}