using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Comprobantes;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Infrastructure.Persistence.Repositories;

public class ComprobanteRepository(AppDbContext context)
    : BaseRepository<Comprobante>(context), IComprobanteRepository
{
    public async Task<Comprobante?> GetByNumeroAsync(
        long sucursalId,
        long tipoComprobanteId,
        short prefijo,
        long numero,
        CancellationToken ct = default) =>
        await DbSet
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x =>
                x.SucursalId        == sucursalId        &&
                x.TipoComprobanteId == tipoComprobanteId &&
                x.Numero.Prefijo    == prefijo           &&
                x.Numero.Numero     == numero,
                ct);

    public async Task<long> GetProximoNumeroAsync(
        long sucursalId,
        long tipoComprobanteId,
        short prefijo,
        CancellationToken ct = default)
    {
        var ultimo = await DbSet
            .Where(x =>
                x.SucursalId        == sucursalId        &&
                x.TipoComprobanteId == tipoComprobanteId &&
                x.Numero.Prefijo    == prefijo)
            .MaxAsync(x => (long?)x.Numero.Numero, ct);

        return (ultimo ?? 0) + 1;
    }

    public async Task<PagedResult<Comprobante>> GetPagedAsync(
        int page,
        int pageSize,
        long? sucursalId,
        long? terceroId,
        long? tipoId,
        string? estado,
        DateOnly? desde,
        DateOnly? hasta,
        CancellationToken ct = default)
    {
        var query = DbSet.AsNoTracking();

        if (sucursalId.HasValue)
            query = query.Where(x => x.SucursalId == sucursalId.Value);

        if (terceroId.HasValue)
            query = query.Where(x => x.TerceroId == terceroId.Value);

        if (tipoId.HasValue)
            query = query.Where(x => x.TipoComprobanteId == tipoId.Value);

        if (!string.IsNullOrWhiteSpace(estado))
            query = query.Where(x => x.Estado.ToString() == estado);

        if (desde.HasValue)
            query = query.Where(x => x.Fecha >= desde.Value);

        if (hasta.HasValue)
            query = query.Where(x => x.Fecha <= hasta.Value);

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(x => x.Fecha)
            .ThenByDescending(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<Comprobante>(items, page, pageSize, total);
    }

    public override async Task<Comprobante?> GetByIdAsync(long id, CancellationToken ct = default) =>
        await DbSet
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
}