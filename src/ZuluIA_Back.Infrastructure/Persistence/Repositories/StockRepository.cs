using Microsoft.EntityFrameworkCore;
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

    public async Task<IList<StockItem>> GetByItemAsync(
        long itemId,
        CancellationToken ct = default) =>
        await DbSet
            .AsNoTracking()
            .Where(x => x.ItemId == itemId)
            .ToListAsync(ct);
}