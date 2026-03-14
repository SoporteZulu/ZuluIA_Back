using MediatR;
using ZuluIA_Back.Application.Features.OrdenesPreparacion.DTOs;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Application.Features.OrdenesPreparacion.Queries;

public record GetOrdenesPreparacionPagedQuery(
    int Page,
    int PageSize,
    long? SucursalId,
    long? TerceroId,
    EstadoOrdenPreparacion? Estado,
    DateOnly? Desde,
    DateOnly? Hasta
) : IRequest<PagedResult<OrdenPreparacionListDto>>;
