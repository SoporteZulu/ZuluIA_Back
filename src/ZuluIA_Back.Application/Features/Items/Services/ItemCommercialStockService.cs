using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Application.Features.Items.Services;

public sealed class ItemCommercialStockService(IApplicationDbContext db)
{
    public async Task<ItemCommercialStockSnapshot> GetSnapshotAsync(long itemId, CancellationToken ct)
    {
        var snapshots = await GetSnapshotsAsync([itemId], ct);
        return snapshots.GetValueOrDefault(itemId);
    }

    public async Task<IReadOnlyDictionary<long, ItemCommercialStockSnapshot>> GetSnapshotsAsync(
        IReadOnlyCollection<long> itemIds,
        CancellationToken ct)
    {
        if (itemIds.Count == 0)
            return new Dictionary<long, ItemCommercialStockSnapshot>();

        var stockFisico = await db.Stock
            .AsNoTracking()
            .Where(x => itemIds.Contains(x.ItemId))
            .GroupBy(x => x.ItemId)
            .Select(x => new { ItemId = x.Key, Cantidad = x.Sum(s => s.Cantidad) })
            .ToDictionaryAsync(x => x.ItemId, x => x.Cantidad, ct);

        var stockOperativoRows = await (
            from ci in db.ComprobantesItems.AsNoTracking()
            join c in db.Comprobantes.AsNoTracking() on ci.ComprobanteId equals c.Id
            join tc in db.TiposComprobante.AsNoTracking() on c.TipoComprobanteId equals tc.Id
            where itemIds.Contains(ci.ItemId)
                  && tc.EsVenta
                  && !tc.AfectaStock
                  && c.Estado != EstadoComprobante.Anulado
                  && c.Estado != EstadoComprobante.Convertido
            select new
            {
                ci.ItemId,
                c.Estado,
                CantidadOperativa = ci.Cantidad - ci.CantidadBonificada
            })
            .ToListAsync(ct);

        var snapshots = itemIds.ToDictionary(x => x, _ => default(ItemCommercialStockSnapshot));

        foreach (var grouping in stockOperativoRows.GroupBy(x => x.ItemId))
        {
            var snapshot = snapshots[grouping.Key];
            snapshot.StockReservado = grouping
                .Where(x => x.Estado == EstadoComprobante.Borrador)
                .Sum(x => x.CantidadOperativa);
            snapshot.StockComprometido = grouping
                .Where(x => x.Estado == EstadoComprobante.Emitido ||
                            x.Estado == EstadoComprobante.PagadoParcial ||
                            x.Estado == EstadoComprobante.Pagado)
                .Sum(x => x.CantidadOperativa);
            snapshots[grouping.Key] = snapshot;
        }

        foreach (var itemId in itemIds)
        {
            var snapshot = snapshots[itemId];
            snapshot.Stock = stockFisico.GetValueOrDefault(itemId);
            snapshots[itemId] = snapshot;
        }

        return snapshots;
    }
}

public struct ItemCommercialStockSnapshot
{
    public decimal Stock { get; set; }
    public decimal StockComprometido { get; set; }
    public decimal StockReservado { get; set; }
    public decimal StockDisponible => Stock - StockComprometido - StockReservado;
}