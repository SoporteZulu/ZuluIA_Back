using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Items.DTOs;
using ZuluIA_Back.Application.Features.ListasPrecios.Services;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Items.Queries;

public class GetItemPrecioQueryHandler(
    IItemRepository itemRepo,
    IApplicationDbContext db,
    PrecioListaResolutionService precioListaResolutionService)
    : IRequestHandler<GetItemPrecioQuery, ItemPrecioDto?>
{
    public async Task<ItemPrecioDto?> Handle(
        GetItemPrecioQuery request,
        CancellationToken ct)
    {
        var item = await itemRepo.GetByIdAsync(request.ItemId, ct);
        if (item is null) return null;

        var alicuota = await db.AlicuotasIva
            .AsNoTracking()
            .Where(x => x.Id == item.AlicuotaIvaId)
            .Select(x => new { x.Porcentaje })
            .FirstOrDefaultAsync(ct);

        var precioVenta = item.PrecioVenta;

        if (request.MonedaId.HasValue && request.Fecha.HasValue)
        {
            var precioResuelto = await precioListaResolutionService.ResolveAsync(
                request.ItemId,
                request.MonedaId.Value,
                request.Fecha.Value,
                request.TerceroId,
                request.ListaPreciosId,
                request.CanalVentaId,
                request.VendedorId,
                item.CategoriaId,
                ct: ct);

            if (precioResuelto is not null)
            {
                precioVenta = precioResuelto.PrecioFinal;
            }
        }

        return new ItemPrecioDto
        {
            Id                    = item.Id,
            Codigo                = item.Codigo,
            Descripcion           = item.Descripcion,
            UnidadMedidaId        = item.UnidadMedidaId,
            AlicuotaIvaId         = item.AlicuotaIvaId,
            AlicuotaIvaPorcentaje = alicuota?.Porcentaje ?? 0,
            PrecioCosto           = item.PrecioCosto,
            PrecioVenta           = precioVenta,
            MonedaId              = item.MonedaId,
            ManejaStock           = item.ManejaStock
        };
    }
}