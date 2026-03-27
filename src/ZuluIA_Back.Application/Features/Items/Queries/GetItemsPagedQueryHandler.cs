using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Items.DTOs;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Items.Queries;

public class GetItemsPagedQueryHandler(
    IItemRepository repo,
    IApplicationDbContext db)
    : IRequestHandler<GetItemsPagedQuery, PagedResult<ItemListDto>>
{
    public async Task<PagedResult<ItemListDto>> Handle(
        GetItemsPagedQuery request,
        CancellationToken ct)
    {
        var result = await repo.GetPagedAsync(
            request.Page,
            request.PageSize,
            request.Search,
            request.CategoriaId,
            request.SoloActivos,
            request.SoloConStock,
            ct);

        // Enriquecer con datos de lookup
        var categoriaIds = result.Items
            .Where(x => x.CategoriaId.HasValue)
            .Select(x => x.CategoriaId!.Value)
            .Distinct()
            .ToList();

        var unidadIds = result.Items
            .Select(x => x.UnidadMedidaId)
            .Distinct()
            .ToList();

        var marcaIds = result.Items
            .Where(x => x.MarcaId.HasValue)
            .Select(x => x.MarcaId!.Value)
            .Distinct()
            .ToList();

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
                .Where(x => marcaIds.Contains(x.Id))
                .Select(x => new KeyValuePair<long, string>(x.Id, x.Descripcion))
                .ToDictionaryAsync(x => x.Key, x => x.Value, ct)
            : new Dictionary<long, string>();

        var unidades = unidadIds.Count > 0
            ? await db.UnidadesMedida
                .AsNoTracking()
                .Where(x => unidadIds.Contains(x.Id))
                .Select(x => new KeyValuePair<long, string>(x.Id, x.Descripcion))
                .ToDictionaryAsync(x => x.Key, x => x.Value, ct)
            : new Dictionary<long, string>();

        var dtos = result.Items.Select(i => new ItemListDto
        {
            Id                      = i.Id,
            Codigo                  = i.Codigo,
            CodigoBarras            = i.CodigoBarras,
            Descripcion             = i.Descripcion,
            CategoriaId             = i.CategoriaId,
            CategoriaDescripcion    = i.CategoriaId.HasValue
                ? categorias.GetValueOrDefault(i.CategoriaId.Value)
                : null,
            MarcaId                 = i.MarcaId,
            MarcaDescripcion        = i.MarcaId.HasValue
                ? marcas.GetValueOrDefault(i.MarcaId.Value)
                : null,
            UnidadMedidaId          = i.UnidadMedidaId,
            UnidadMedidaDescripcion = unidades.GetValueOrDefault(i.UnidadMedidaId),
            EsProducto              = i.EsProducto,
            EsServicio              = i.EsServicio,
            ManejaStock             = i.ManejaStock,
            PrecioVenta             = i.PrecioVenta,
            Activo                  = i.Activo
        }).ToList();

        return new PagedResult<ItemListDto>(
            dtos, result.Page, result.PageSize, result.TotalCount);
    }
}
