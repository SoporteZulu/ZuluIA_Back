using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Domain.Entities.Usuarios;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Infrastructure.Persistence.Repositories;

public class MenuRepository(AppDbContext context)
    : BaseRepository<MenuItem>(context), IMenuRepository
{
    public async Task<IReadOnlyList<MenuItem>> GetArbolCompletoAsync(
        CancellationToken ct = default) =>
        await DbSet
            .AsNoTracking()
            .Where(x => x.Activo)
            .OrderBy(x => x.Nivel)
            .ThenBy(x => x.Orden)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<MenuItem>> GetMenuUsuarioAsync(
        long usuarioId,
        CancellationToken ct = default)
    {
        var menuIds = await context.MenuUsuario
            .AsNoTracking()
            .Where(x => x.UsuarioId == usuarioId)
            .Select(x => x.MenuId)
            .ToListAsync(ct);

        return await DbSet
            .AsNoTracking()
            .Where(x => menuIds.Contains(x.Id) && x.Activo)
            .OrderBy(x => x.Nivel)
            .ThenBy(x => x.Orden)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<long>> GetMenuIdsUsuarioAsync(
        long usuarioId,
        CancellationToken ct = default) =>
        await context.MenuUsuario
            .AsNoTracking()
            .Where(x => x.UsuarioId == usuarioId)
            .Select(x => x.MenuId)
            .ToListAsync(ct);
}