using MediatR;
using ZuluIA_Back.Application.Features.ListasPrecios.DTOs;

namespace ZuluIA_Back.Application.Features.ListasPrecios.Queries;

/// <summary>
/// Retorna el detalle completo de una lista de precios con todos sus ítems.
/// </summary>
public record GetListaPreciosByIdQuery(long Id) : IRequest<ListaPreciosDetalleDto?>;