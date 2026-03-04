using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Items.DTOs;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Items.Queries;

public class GetItemPrecioQueryHandler(
    IItemRepository itemRepo,
    IListaPreciosRepository preciosRepo,
    IApplicationDbContext db)
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

        // Resolver precio desde lista si se especificó
        if (request.ListaPreciosId.HasValue)
        {
            var precioLista = await preciosRepo.GetPrecioItemAsync(
                request.ListaPreciosId.Value, request.ItemId, ct);

            if (precioLista is not null)
                precioVenta = precioLista.PrecioFinal;
        }
        else if (request.MonedaId.HasValue && request.Fecha.HasValue)
        {
            // Resolver automáticamente por moneda y fecha
            var precioAuto = await preciosRepo.ResolverPrecioItemAsync(
                request.ItemId,
                request.MonedaId.Value,
                request.Fecha.Value,
                ct);

            if (precioAuto is not null)
                precioVenta = precioAuto.PrecioFinal;
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