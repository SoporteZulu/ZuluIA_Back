using MediatR;
using ZuluIA_Back.Application.Features.Finanzas.DTOs;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Finanzas.Queries;

public record GetMovimientosCtaCteQuery(
    int Page,
    int PageSize,
    long TerceroId,
    long? SucursalId,
    long? MonedaId,
    DateOnly? Desde,
    DateOnly? Hasta)
    : IRequest<PagedResult<MovimientoCtaCteDto>>;