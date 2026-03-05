using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Comprobantes;
using ZuluIA_Back.Domain.Enums;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Infrastructure.Persistence.Repositories;

public class ComprobanteRepository(AppDbContext context)
    : BaseRepository<Comprobante>(context), IComprobanteRepository
{
    public async Task<PagedResult<Comprobante>> GetPagedAsync(
        int page,
        int pageSize,
        long? sucursalId,
        long? terceroId,
        long? tipoComprobanteId,
        EstadoComprobante? estado,
        DateOnly? desde,
        DateOnly? hasta,
        CancellationToken ct = default)
    {
        var query = DbSet.AsNoTracking();

        if (sucursalId.HasValue)
            query = query.Where(x => x.SucursalId == sucursalId.Value);

        if (terceroId.HasValue)
            query = query.Where(x => x.TerceroId == terceroId.Value);

        if (tipoComprobanteId.HasValue)
            query = query.Where(x => x.TipoComprobanteId == tipoComprobanteId.Value);

        if (estado.HasValue)
            query = query.Where(x => x.Estado == estado.Value);

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

    public async Task<Comprobante?> GetByIdConItemsAsync(
        long id,
        CancellationToken ct = default) =>
        await DbSet
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<Comprobante?> GetByNumeroAsync(
        long sucursalId,
        long tipoComprobanteId,
        short prefijo,
        long numero,
        CancellationToken ct = default) =>
        await DbSet
            .Include(x => x.Items)
            .AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.SucursalId         == sucursalId         &&
                x.TipoComprobanteId  == tipoComprobanteId  &&
                x.Numero.Prefijo     == prefijo            &&
                x.Numero.Numero      == numero,
                ct);

    public async Task<IReadOnlyList<Comprobante>> GetSaldoPendienteByTerceroAsync(
        long terceroId,
        long? sucursalId,
        CancellationToken ct = default)
    {
        var query = DbSet
            .AsNoTracking()
            .Where(x =>
                x.TerceroId == terceroId &&
                x.Saldo > 0 &&
                x.Estado != EstadoComprobante.Anulado);

        if (sucursalId.HasValue)
            query = query.Where(x => x.SucursalId == sucursalId.Value);

        return await query
            .OrderBy(x => x.Fecha)
            .ToListAsync(ct);
    }

    public async Task<long> GetProximoNumeroAsync(
        long puntoFacturacionId,
        long tipoComprobanteId,
        CancellationToken ct = default)
    {
        var ultimo = await DbSet
            .AsNoTracking()
            .Where(x =>
                x.PuntoFacturacionId == puntoFacturacionId &&
                x.TipoComprobanteId  == tipoComprobanteId  &&
                x.Estado             != EstadoComprobante.Anulado)
            .MaxAsync(x => (long?)x.Numero.Numero, ct);

        return (ultimo ?? 0) + 1;
    }

    public override async Task<Comprobante?> GetByIdAsync(long id, CancellationToken ct = default) =>
        await DbSet
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
}
