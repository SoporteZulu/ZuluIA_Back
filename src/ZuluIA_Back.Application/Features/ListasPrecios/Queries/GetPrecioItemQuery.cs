using MediatR;
using ZuluIA_Back.Application.Features.ListasPrecios.DTOs;

namespace ZuluIA_Back.Application.Features.ListasPrecios.Queries;

/// <summary>
/// Retorna el precio de un ítem específico dentro de una lista.
/// </summary>
public record GetPrecioItemQuery(long ListaId, long ItemId) : IRequest<ListaPreciosItemDto?>;