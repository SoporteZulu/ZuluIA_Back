using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Stock;
using ZuluIA_Back.Domain.Enums;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Infrastructure.Persistence.Repositories;

public class MovimientoStockRepository(AppDbContext context)
    : BaseRepository<MovimientoStock>(context), IMovimientoStockRepository
{
    public async Task<PagedResult<MovimientoStock>> GetPagedAsync(
        int page,
        int pageSize,
        long? itemId,
        long? depositoId,
        TipoMovimientoStock? tipo,
        DateOnly? desde,
        DateOnly? hasta,
        CancellationToken ct = default)
    {
        var query = DbSet.AsNoTracking();

        if (itemId.HasValue)
            query = query.Where(x => x.ItemId == itemId.Value);

        if (depositoId.HasValue)
            query = query.Where(x => x.DepositoId == depositoId.Value);

        if (tipo.HasValue)
            query = query.Where(x => x.TipoMovimiento == tipo.Value);

        if (desde.HasValue)
        {
            var desdeUtc = desde.Value.ToDateTime(TimeOnly.MinValue,
                DateTimeKind.Utc);
            query = query.Where(x => x.Fecha >= desdeUtc);
        }

        if (hasta.HasValue)
        {
            var hastaUtc = hasta.Value.ToDateTime(TimeOnly.MaxValue,
                DateTimeKind.Utc);
            query = query.Where(x => x.Fecha <= hastaUtc);
        }

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(x => x.Fecha)
            .ThenByDescending(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<MovimientoStock>(items, page, pageSize, total);
    }

    public async Task<IReadOnlyList<MovimientoStock>> GetByOrigenAsync(
        string origenTabla,
        long origenId,
        CancellationToken ct = default) =>
        await DbSet
            .AsNoTracking()
            .Where(x =>
                x.OrigenTabla == origenTabla &&
                x.OrigenId    == origenId)
            .OrderByDescending(x => x.Fecha)
            .ToListAsync(ct);
}