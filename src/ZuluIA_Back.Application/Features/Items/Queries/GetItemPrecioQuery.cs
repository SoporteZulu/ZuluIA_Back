using MediatR;
using ZuluIA_Back.Application.Features.Items.DTOs;

namespace ZuluIA_Back.Application.Features.Items.Queries;

/// <summary>
/// Retorna el precio de un ítem, opcionalmente resolviendo
/// el precio desde una lista de precios para una fecha dada.
/// </summary>
public record GetItemPrecioQuery(
    long ItemId,
    long? ListaPreciosId,
    long? MonedaId,
    DateOnly? Fecha,
    long? TerceroId = null,
    long? CanalVentaId = null,
    long? VendedorId = null)
    : IRequest<ItemPrecioDto?>;