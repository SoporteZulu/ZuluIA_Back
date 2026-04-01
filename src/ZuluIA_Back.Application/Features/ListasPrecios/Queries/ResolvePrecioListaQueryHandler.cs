using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.ListasPrecios.DTOs;
using ZuluIA_Back.Application.Features.ListasPrecios.Services;

namespace ZuluIA_Back.Application.Features.ListasPrecios.Queries;

public class ResolvePrecioListaQueryHandler(
    IApplicationDbContext db,
    PrecioListaResolutionService precioListaResolutionService)
    : IRequestHandler<ResolvePrecioListaQuery, PrecioListaResueltoDto?>
{
    public async Task<PrecioListaResueltoDto?> Handle(
        ResolvePrecioListaQuery request,
        CancellationToken ct)
    {
        var categoriaItemId = await db.Items
            .AsNoTracking()
            .Where(x => x.Id == request.ItemId)
            .Select(x => x.CategoriaId)
            .FirstOrDefaultAsync(ct);

        var precio = await precioListaResolutionService.ResolveAsync(
            request.ItemId,
            request.MonedaId,
            request.Fecha,
            request.TerceroId,
            request.ListaPreciosId,
            request.CanalVentaId,
            request.VendedorId,
            categoriaItemId,
            ct: ct);

        return precio is null
            ? null
            : new PrecioListaResueltoDto
            {
                Precio = precio.Precio,
                DescuentoPct = precio.DescuentoPct,
                PrecioFinal = precio.PrecioFinal,
                ListaPreciosId = precio.ListaPreciosId,
                Origen = precio.Origen,
                PromocionId = precio.PromocionId
            };
    }
}
