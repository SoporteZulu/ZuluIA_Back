using MediatR;
using ZuluIA_Back.Application.Features.ListasPrecios.DTOs;

namespace ZuluIA_Back.Application.Features.ListasPrecios.Queries;

/// <summary>
/// Retorna el listado de listas de precios activas.
/// Opcionalmente filtra por fecha de vigencia.
/// </summary>
public record GetListasPreciosQuery(DateOnly? Fecha = null) : IRequest<IReadOnlyList<ListaPreciosDto>>;