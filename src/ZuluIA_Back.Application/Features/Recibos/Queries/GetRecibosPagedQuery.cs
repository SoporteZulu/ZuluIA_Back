using MediatR;
using ZuluIA_Back.Application.Features.Recibos.DTOs;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Recibos.Queries;

public record GetRecibosPagedQuery(
    int Page,
    int PageSize,
    long? SucursalId,
    long? TerceroId,
    DateOnly? Desde,
    DateOnly? Hasta)
    : IRequest<PagedResult<ReciboListDto>>;
