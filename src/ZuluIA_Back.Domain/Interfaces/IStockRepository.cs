using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Stock;

namespace ZuluIA_Back.Domain.Interfaces;

public interface IStockRepository : IRepository<StockItem>
{
    /// <summary>
    /// Obtiene el registro de stock para un ítem y depósito.
    /// </summary>
    Task<StockItem?> GetByItemDepositoAsync(
        long itemId,
        long depositoId,
        CancellationToken ct = default);

    /// <summary>
    /// Obtiene el saldo actual de stock para un ítem y depósito.
    /// </summary>
    Task<decimal> GetSaldoAsync(
        long itemId,
        long depositoId,
        CancellationToken ct = default);

    /// <summary>
    /// Obtiene todos los registros de stock para un ítem.
    /// </summary>
    Task<IReadOnlyList<StockItem>> GetByItemAsync(
        long itemId,
        CancellationToken ct = default);

    /// <summary>
    /// Obtiene todos los registros de stock para un depósito.
    /// </summary>
    Task<IReadOnlyList<StockItem>> GetByDepositoAsync(
        long depositoId,
        CancellationToken ct = default);

    /// <summary>
    /// Obtiene los ítems con stock bajo el mínimo, filtrando opcionalmente por sucursal y depósito.
    /// </summary>
    Task<IReadOnlyList<StockItem>> GetBajoMinimoAsync(
        long? sucursalId,
        long? depositoId,
        CancellationToken ct = default);

    /// <summary>
    /// Obtiene o crea el registro de stock para un ítem/depósito.
    /// </summary>
    Task<StockItem> GetOrCreateAsync(
        long itemId,
        long depositoId,
        CancellationToken ct = default);
}
