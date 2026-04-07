using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Extensions;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Items.Services;
using ZuluIA_Back.Domain.Entities.Items;

namespace ZuluIA_Back.Application.Features.Ventas.Commands;

internal static class VentaItemValidationHelper
{
    public static async Task<Dictionary<long, Item>> LoadItemsByIdAsync(
        IApplicationDbContext db,
        IReadOnlyCollection<long> itemIds,
        CancellationToken ct)
    {
        if (itemIds.Count == 0)
            return [];

        return await db.Items
            .AsNoTrackingSafe()
            .Where(x => itemIds.Contains(x.Id))
            .ToDictionarySafeAsync(x => x.Id, ct);
    }

    public static string? ValidateItemsVendibles(
        IReadOnlyDictionary<long, Item> itemsById,
        IReadOnlyDictionary<long, decimal> cantidadesSolicitadas)
    {
        foreach (var (itemId, cantidad) in cantidadesSolicitadas)
        {
            if (!itemsById.TryGetValue(itemId, out var item))
                return $"No se encontró el ítem ID {itemId}.";

            if (cantidad <= 0)
                continue;

            if (!item.Activo)
                return $"El ítem '{item.Codigo} - {item.Descripcion}' está inactivo.";

            if (!item.AplicaVentas || item.EsFinanciero || (!item.EsProducto && !item.EsServicio))
                return $"El ítem '{item.Codigo} - {item.Descripcion}' no está habilitado para ventas.";
        }

        return null;
    }

    public static async Task<string?> ValidateStockDisponibleAsync(
        ItemCommercialStockService itemCommercialStockService,
        IReadOnlyDictionary<long, Item> itemsById,
        IReadOnlyDictionary<long, decimal> cantidadesSolicitadas,
        IReadOnlyDictionary<long, decimal>? stockYaReservadoPorComprobante,
        CancellationToken ct)
    {
        var itemIdsConStock = cantidadesSolicitadas
            .Where(x => x.Value > 0 && itemsById.TryGetValue(x.Key, out var item) && item.ManejaStock)
            .Select(x => x.Key)
            .ToList();

        if (itemIdsConStock.Count == 0)
            return null;

        var snapshots = await itemCommercialStockService.GetSnapshotsAsync(itemIdsConStock, ct);

        foreach (var itemId in itemIdsConStock)
        {
            var item = itemsById[itemId];
            var snapshot = snapshots.GetValueOrDefault(itemId);
            var stockPropioReservado = stockYaReservadoPorComprobante?.GetValueOrDefault(itemId) ?? 0m;
            var stockDisponible = snapshot.StockDisponible + stockPropioReservado;
            var cantidadRequerida = cantidadesSolicitadas[itemId];

            if (stockDisponible < cantidadRequerida)
                return $"Stock insuficiente para el ítem '{item.Codigo} - {item.Descripcion}'. Disponible: {stockDisponible}, requerido: {cantidadRequerida}.";
        }

        return null;
    }
}
