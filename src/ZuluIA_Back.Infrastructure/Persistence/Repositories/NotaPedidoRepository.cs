using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Ventas;
using ZuluIA_Back.Domain.Enums;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Infrastructure.Persistence.Repositories;

public class NotaPedidoRepository(AppDbContext context)
    : BaseRepository<NotaPedido>(context), INotaPedidoRepository
{
    public async Task<PagedResult<NotaPedido>> GetPagedAsync(
        int page, int pageSize,
        long? sucursalId, long? terceroId, string? estado,
        CancellationToken ct = default)
    {
        var query = DbSet.AsNoTracking();
        if (sucursalId.HasValue) query = query.Where(x => x.SucursalId == sucursalId.Value);
        if (terceroId.HasValue)  query = query.Where(x => x.TerceroId  == terceroId.Value);
        if (!string.IsNullOrEmpty(estado) && Enum.TryParse<EstadoNotaPedido>(estado, true, out var estadoEnum))
            query = query.Where(x => x.Estado == estadoEnum);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(x => x.Fecha).ThenByDescending(x => x.Id)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<NotaPedido>(items, page, pageSize, total);
    }

    public async Task<NotaPedido?> GetByIdConItemsAsync(long id, CancellationToken ct = default) =>
        await DbSet.Include(x => x.Items).FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<IReadOnlyList<NotaPedido>> GetPendientesAsync(long sucursalId, CancellationToken ct = default) =>
        await DbSet.AsNoTracking()
            .Include(x => x.Items)
            .Where(x => x.SucursalId == sucursalId
                     && (x.Estado == EstadoNotaPedido.Abierta || x.Estado == EstadoNotaPedido.Parcial))
            .OrderBy(x => x.FechaVencimiento).ThenBy(x => x.Id)
            .ToListAsync(ct);
}
