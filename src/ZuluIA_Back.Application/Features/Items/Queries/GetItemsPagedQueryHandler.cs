using AutoMapper;
using MediatR;
using ZuluIA_Back.Application.Features.Items.DTOs;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Items.Queries;

public class GetItemsPagedQueryHandler(
    IItemRepository repo,
    IMapper mapper)
    : IRequestHandler<GetItemsPagedQuery, PagedResult<ItemListDto>>
{
    public async Task<PagedResult<ItemListDto>> Handle(GetItemsPagedQuery request, CancellationToken ct)
    {
        var result = await repo.GetPagedAsync(
            request.Page,
            request.PageSize,
            request.Search,
            request.SoloProductos,
            request.SoloServicios,
            ct);

        var items = mapper.Map<IReadOnlyList<ItemListDto>>(result.Items);
        return new PagedResult<ItemListDto>(items, result.Page, result.PageSize, result.TotalCount);
    }
}