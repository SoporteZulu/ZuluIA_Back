using MediatR;
using ZuluIA_Back.Application.Features.ListasPrecios.DTOs;

namespace ZuluIA_Back.Application.Features.ListasPrecios.Queries;

public record ResolvePrecioListaQuery(
    long ItemId,
    long MonedaId,
    DateOnly Fecha,
    long? TerceroId = null,
    long? ListaPreciosId = null,
    long? CanalVentaId = null,
    long? VendedorId = null)
    : IRequest<PrecioListaResueltoDto?>;
