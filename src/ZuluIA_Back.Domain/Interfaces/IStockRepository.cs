using ZuluIA_Back.Domain.Entities.Stock;

namespace ZuluIA_Back.Domain.Interfaces;

public interface IStockRepository : IRepository<StockItem>
{
    Task<StockItem?> GetByItemDepositoAsync(long itemId, long depositoId, CancellationToken ct = default);
    Task<decimal> GetSaldoAsync(long itemId, long depositoId, CancellationToken ct = default);
    Task<IList<StockItem>> GetByItemAsync(long itemId, CancellationToken ct = default);
}