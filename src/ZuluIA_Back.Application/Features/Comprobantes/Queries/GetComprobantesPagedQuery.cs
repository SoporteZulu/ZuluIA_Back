using MediatR;
using ZuluIA_Back.Application.Features.Comprobantes.DTOs;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Comprobantes.Queries;

public record GetComprobantesPagedQuery(
    int Page = 1,
    int PageSize = 20,
    long? SucursalId = null,
    long? TerceroId = null,
    long? TipoId = null,
    string? Estado = null,
    DateOnly? Desde = null,
    DateOnly? Hasta = null
) : IRequest<PagedResult<ComprobanteListDto>>;