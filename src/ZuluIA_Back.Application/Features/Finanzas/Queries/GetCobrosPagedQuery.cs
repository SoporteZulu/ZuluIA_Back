using MediatR;
using ZuluIA_Back.Application.Features.Finanzas.DTOs;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Finanzas.Queries;

public record GetCobrosPagedQuery(
    int Page,
    int PageSize,
    long? SucursalId,
    long? TerceroId,
    DateOnly? Desde,
    DateOnly? Hasta)
    : IRequest<PagedResult<CobroListDto>>;