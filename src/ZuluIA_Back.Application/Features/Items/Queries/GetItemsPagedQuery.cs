using MediatR;
using ZuluIA_Back.Application.Features.Items.DTOs;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Items.Queries;

public record GetItemsPagedQuery(
    int Page = 1,
    int PageSize = 20,
    string? Search = null,
    bool? SoloProductos = null,
    bool? SoloServicios = null
) : IRequest<PagedResult<ItemListDto>>;