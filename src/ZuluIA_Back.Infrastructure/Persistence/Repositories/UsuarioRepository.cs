using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Usuarios;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Infrastructure.Persistence.Repositories;

public class UsuarioRepository(AppDbContext context)
    : BaseRepository<Usuario>(context), IUsuarioRepository
{
    public async Task<Usuario?> GetByUserNameAsync(
        string userName,
        CancellationToken ct = default) =>
        await DbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.UserName == userName.Trim().ToLowerInvariant(), ct);

    public async Task<Usuario?> GetBySupabaseIdAsync(
        Guid supabaseId,
        CancellationToken ct = default) =>
        await DbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.SupabaseUserId == supabaseId, ct);

    public async Task<Usuario?> GetByIdConSucursalesAsync(
        long id,
        CancellationToken ct = default) =>
        await DbSet
            .Include(x => x.Sucursales)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<bool> ExisteUserNameAsync(
        string userName,
        long? excludeId = null,
        CancellationToken ct = default)
    {
        var query = DbSet
            .Where(x => x.UserName == userName.Trim().ToLowerInvariant());

        if (excludeId.HasValue)
            query = query.Where(x => x.Id != excludeId.Value);

        return await query.AnyAsync(ct);
    }

    public async Task<PagedResult<Usuario>> GetPagedAsync(
        int page,
        int pageSize,
        string? search,
        bool? soloActivos,
        CancellationToken ct = default)
    {
        var query = DbSet.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLower();
            query = query.Where(x =>
                x.UserName.Contains(term) ||
                (x.NombreCompleto != null && x.NombreCompleto.ToLower().Contains(term)) ||
                (x.Email != null && x.Email.ToLower().Contains(term)));
        }

        if (soloActivos.HasValue)
            query = query.Where(x => x.Activo == soloActivos.Value);

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderBy(x => x.UserName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<Usuario>(items, page, pageSize, total);
    }
}