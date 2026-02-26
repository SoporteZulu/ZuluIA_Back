using MediatR;
using ZuluIA_Back.Application.Features.Terceros.DTOs;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Terceros.Queries;

public record GetTercerosPagedQuery(
    int Page = 1,
    int PageSize = 20,
    string? Search = null,
    bool? SoloClientes = null,
    bool? SoloProveedores = null
) : IRequest<PagedResult<TerceroListDto>>;