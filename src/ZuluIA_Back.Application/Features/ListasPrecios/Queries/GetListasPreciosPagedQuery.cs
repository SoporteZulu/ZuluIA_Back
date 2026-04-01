using MediatR;
using ZuluIA_Back.Application.Features.ListasPrecios.DTOs;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.ListasPrecios.Queries;

/// <summary>
/// Retorna las listas de precios paginadas con filtros operativos.
/// </summary>
public record GetListasPreciosPagedQuery(
    int Page = 1,
    int PageSize = 20,
    string? Search = null,
    long? MonedaId = null,
    bool? SoloActivas = null,
    bool? EsPorDefecto = null,
    bool? SoloVigentes = null,
    DateOnly? Fecha = null,
    long? ListaPadreId = null)
    : IRequest<PagedResult<ListaPreciosDto>>;
