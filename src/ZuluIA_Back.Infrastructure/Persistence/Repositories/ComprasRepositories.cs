using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Compras;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Infrastructure.Persistence.Repositories;

public class RequisicionCompraRepository(AppDbContext context)
    : BaseRepository<RequisicionCompra>(context), IRequisicionCompraRepository
{
    public async Task<PagedResult<RequisicionCompra>> GetPagedAsync(
        int page, int pageSize,
        long? sucursalId, long? solicitanteId, string? estado,
        CancellationToken ct = default)
    {
        var query = DbSet.AsNoTracking().Where(x => x.DeletedAt == null);
        if (sucursalId.HasValue)  query = query.Where(x => x.SucursalId    == sucursalId.Value);
        if (solicitanteId.HasValue) query = query.Where(x => x.SolicitanteId == solicitanteId.Value);
        if (!string.IsNullOrEmpty(estado))
            query = query.Where(x => x.Estado.ToString() == estado.ToUpperInvariant());

        var total = await query.CountAsync(ct);
        var items = await query
            .Include(x => x.Items)
            .OrderByDescending(x => x.Fecha).ThenByDescending(x => x.Id)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<RequisicionCompra>(items, page, pageSize, total);
    }

    public async Task<RequisicionCompra?> GetByIdConItemsAsync(long id, CancellationToken ct = default) =>
        await DbSet.Include(x => x.Items).FirstOrDefaultAsync(x => x.Id == id && x.DeletedAt == null, ct);
}

public class CotizacionCompraRepository(AppDbContext context)
    : BaseRepository<CotizacionCompra>(context), ICotizacionCompraRepository
{
    public async Task<PagedResult<CotizacionCompra>> GetPagedAsync(
        int page, int pageSize,
        long? sucursalId, long? proveedorId, string? estado,
        CancellationToken ct = default)
    {
        var query = DbSet.AsNoTracking();
        if (sucursalId.HasValue)  query = query.Where(x => x.SucursalId  == sucursalId.Value);
        if (proveedorId.HasValue) query = query.Where(x => x.ProveedorId == proveedorId.Value);
        if (!string.IsNullOrEmpty(estado))
            query = query.Where(x => x.Estado.ToString() == estado.ToUpperInvariant());

        var total = await query.CountAsync(ct);
        var items = await query
            .Include(x => x.Items)
            .OrderByDescending(x => x.Fecha).ThenByDescending(x => x.Id)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<CotizacionCompra>(items, page, pageSize, total);
    }

    public async Task<CotizacionCompra?> GetByIdConItemsAsync(long id, CancellationToken ct = default) =>
        await DbSet.Include(x => x.Items).FirstOrDefaultAsync(x => x.Id == id, ct);
}
