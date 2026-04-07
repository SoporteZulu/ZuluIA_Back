using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Items.DTOs;
using ZuluIA_Back.Application.Features.Items.Services;

namespace ZuluIA_Back.Application.Features.Items.Queries;

/// <summary>
/// Devuelve un selector comercial liviano de ítems vendibles para ventas.
/// </summary>
public record GetItemsVendiblesQuery(
    string? Search = null,
    bool SoloConStock = false,
    int Take = 20) : IRequest<IReadOnlyList<ItemSelectorDto>>;

public class GetItemsVendiblesQueryHandler(
    IApplicationDbContext db,
    ItemCommercialStockService itemCommercialStockService)
    : IRequestHandler<GetItemsVendiblesQuery, IReadOnlyList<ItemSelectorDto>>
{
    public async Task<IReadOnlyList<ItemSelectorDto>> Handle(
        GetItemsVendiblesQuery request,
        CancellationToken ct)
    {
        var take = Math.Clamp(request.Take, 1, 100);

        var query = db.Items
            .AsNoTracking()
            .Where(x => x.Activo && x.AplicaVentas && !x.EsFinanciero && (x.EsProducto || x.EsServicio));

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim();
            var termUpper = term.ToUpperInvariant();
            var termLower = term.ToLowerInvariant();

            query = query.Where(x =>
                x.Codigo.Contains(termUpper) ||
                (x.CodigoAlternativo != null && x.CodigoAlternativo.Contains(termUpper)) ||
                (x.CodigoBarras != null && x.CodigoBarras.Contains(term)) ||
                x.Descripcion.ToLower().Contains(termLower));

            query = query
                .OrderByDescending(x => x.Codigo == termUpper)
                .ThenByDescending(x => x.CodigoAlternativo != null && x.CodigoAlternativo == termUpper)
                .ThenByDescending(x => x.CodigoBarras != null && x.CodigoBarras == term)
                .ThenByDescending(x => x.Codigo.StartsWith(termUpper))
                .ThenByDescending(x => x.CodigoAlternativo != null && x.CodigoAlternativo.StartsWith(termUpper))
                .ThenByDescending(x => x.CodigoBarras != null && x.CodigoBarras.StartsWith(term))
                .ThenByDescending(x => x.Descripcion.ToLower().StartsWith(termLower))
                .ThenBy(x => x.Descripcion)
                .ThenBy(x => x.Codigo);
        }
        else
        {
            query = query
                .OrderBy(x => x.Descripcion)
                .ThenBy(x => x.Codigo);
        }

        var items = await query
            .Take(take)
            .ToListAsync(ct);

        if (items.Count == 0)
            return [];

        var itemIds = items.Select(x => x.Id).ToList();
        var unidadIds = items.Select(x => x.UnidadMedidaId).Distinct().ToList();
        var categoriaIds = items.Where(x => x.CategoriaId.HasValue).Select(x => x.CategoriaId!.Value).Distinct().ToList();
        var marcaIds = items.Where(x => x.MarcaId.HasValue).Select(x => x.MarcaId!.Value).Distinct().ToList();

        var unidades = await db.UnidadesMedida
            .AsNoTracking()
            .Where(x => unidadIds.Contains(x.Id))
            .Select(x => new KeyValuePair<long, string>(x.Id, x.Descripcion))
            .ToDictionaryAsync(x => x.Key, x => x.Value, ct);

        var categorias = categoriaIds.Count > 0
            ? await db.CategoriasItems
                .AsNoTracking()
                .Where(x => categoriaIds.Contains(x.Id))
                .Select(x => new KeyValuePair<long, string>(x.Id, x.Descripcion))
                .ToDictionaryAsync(x => x.Key, x => x.Value, ct)
            : new Dictionary<long, string>();

        var marcas = marcaIds.Count > 0
            ? await db.MarcasComerciales
                .AsNoTracking()
                .Where(x => marcaIds.Contains(x.Id) && x.DeletedAt == null)
                .Select(x => new KeyValuePair<long, string>(x.Id, x.Descripcion))
                .ToDictionaryAsync(x => x.Key, x => x.Value, ct)
            : new Dictionary<long, string>();

        var stockSnapshots = await itemCommercialStockService.GetSnapshotsAsync(itemIds, ct);

        var result = items
            .Select(item =>
            {
                var stockSnapshot = stockSnapshots.GetValueOrDefault(item.Id);

                return new ItemSelectorDto
                {
                    Id = item.Id,
                    Codigo = item.Codigo,
                    CodigoAlternativo = item.CodigoAlternativo,
                    CodigoBarras = item.CodigoBarras,
                    Descripcion = item.Descripcion,
                    CategoriaId = item.CategoriaId,
                    CategoriaDescripcion = item.CategoriaId.HasValue ? categorias.GetValueOrDefault(item.CategoriaId.Value) : null,
                    MarcaId = item.MarcaId,
                    MarcaDescripcion = item.MarcaId.HasValue ? marcas.GetValueOrDefault(item.MarcaId.Value) : null,
                    UnidadMedidaId = item.UnidadMedidaId,
                    UnidadMedidaDescripcion = unidades.GetValueOrDefault(item.UnidadMedidaId),
                    AlicuotaIvaId = item.AlicuotaIvaId,
                    PrecioVenta = item.PrecioVenta,
                    StockDisponible = stockSnapshot.StockDisponible,
                    PorcentajeMaximoDescuento = item.PorcentajeMaximoDescuento,
                    ManejaStock = item.ManejaStock,
                    EsVendible = true
                };
            })
            .Where(x => !request.SoloConStock || !x.ManejaStock || x.StockDisponible > 0)
            .ToList();

        return result;
    }
}