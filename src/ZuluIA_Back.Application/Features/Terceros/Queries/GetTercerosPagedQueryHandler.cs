using AutoMapper;
using MediatR;
using ZuluIA_Back.Application.Features.Terceros.DTOs;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Terceros.Queries;

public class GetTercerosPagedQueryHandler(
    ITerceroRepository repo,
    IMapper mapper)
    : IRequestHandler<GetTercerosPagedQuery, PagedResult<TerceroListDto>>
{
    public async Task<PagedResult<TerceroListDto>> Handle(GetTercerosPagedQuery request, CancellationToken ct)
    {
        var result = await repo.GetPagedAsync(
            request.Page,
            request.PageSize,
            request.Search,
            request.SoloClientes,
            request.SoloProveedores,
            ct);

        var items = mapper.Map<IReadOnlyList<TerceroListDto>>(result.Items);
        return new PagedResult<TerceroListDto>(items, result.Page, result.PageSize, result.TotalCount);
    }
}