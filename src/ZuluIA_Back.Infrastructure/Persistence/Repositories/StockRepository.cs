using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Domain.Entities.Items;
using ZuluIA_Back.Domain.Entities.Stock;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Infrastructure.Persistence.Repositories;

public class StockRepository(AppDbContext context)
    : BaseRepository<StockItem>(context), IStockRepository
{
    public async Task<StockItem?> GetByItemDepositoAsync(
        long itemId,
        long depositoId,
        CancellationToken ct = default) =>
        await DbSet
            .FirstOrDefaultAsync(x =>
                x.ItemId     == itemId &&
                x.DepositoId == depositoId,
                ct);

    public async Task<decimal> GetSaldoAsync(
        long itemId,
        long depositoId,
        CancellationToken ct = default)
    {
        var stock = await GetByItemDepositoAsync(itemId, depositoId, ct);
        return stock?.Cantidad ?? 0;
    }

    public async Task<IReadOnlyList<StockItem>> GetByItemAsync(
        long itemId,
        CancellationToken ct = default) =>
        await DbSet
            .AsNoTracking()
            .Where(x => x.ItemId == itemId)
            .OrderBy(x => x.DepositoId)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<StockItem>> GetByDepositoAsync(
        long depositoId,
        CancellationToken ct = default) =>
        await DbSet
            .AsNoTracking()
            .Where(x => x.DepositoId == depositoId)
            .OrderBy(x => x.ItemId)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<StockItem>> GetBajoMinimoAsync(
        long? sucursalId,
        long? depositoId,
        CancellationToken ct = default)
    {
        // Join con items para comparar cantidad vs stock_minimo
        var query =
            from s in context.Stock.AsNoTracking()
            join i in context.Items.AsNoTracking()
                on s.ItemId equals i.Id
            join d in context.Depositos.AsNoTracking()
                on s.DepositoId equals d.Id
            where i.ManejaStock &&
                  i.Activo      &&
                  s.Cantidad < i.StockMinimo
            select s;

        if (depositoId.HasValue)
            query = query.Where(x => x.DepositoId == depositoId.Value);

        if (sucursalId.HasValue)
        {
            var depositoIds = context.Depositos
                .AsNoTracking()
                .Where(d => d.SucursalId == sucursalId.Value && d.Activo)
                .Select(d => d.Id);

            query = query.Where(x => depositoIds.Contains(x.DepositoId));
        }

        return await query
            .OrderBy(x => x.ItemId)
            .ToListAsync(ct);
    }

    public async Task<StockItem> GetOrCreateAsync(
        long itemId,
        long depositoId,
        CancellationToken ct = default)
    {
        var stock = await DbSet
            .FirstOrDefaultAsync(x =>
                x.ItemId     == itemId &&
                x.DepositoId == depositoId,
                ct);

        if (stock is not null)
            return stock;

        stock = StockItem.Crear(itemId, depositoId);
        await DbSet.AddAsync(stock, ct);
        return stock;
    }
}
