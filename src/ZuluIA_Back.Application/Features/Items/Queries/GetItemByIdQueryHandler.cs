using AutoMapper;
using MediatR;
using ZuluIA_Back.Application.Features.Items.DTOs;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Items.Queries;

public class GetItemByIdQueryHandler(
    IItemRepository repo,
    IMapper mapper)
    : IRequestHandler<GetItemByIdQuery, ItemDto?>
{
    public async Task<ItemDto?> Handle(GetItemByIdQuery request, CancellationToken ct)
    {
        var item = await repo.GetByIdAsync(request.Id, ct);
        return item is null ? null : mapper.Map<ItemDto>(item);
    }
}