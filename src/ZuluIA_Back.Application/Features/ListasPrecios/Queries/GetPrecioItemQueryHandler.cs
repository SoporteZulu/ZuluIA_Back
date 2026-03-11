using AutoMapper;
using MediatR;
using ZuluIA_Back.Application.Features.ListasPrecios.DTOs;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.ListasPrecios.Queries;

public class GetPrecioItemQueryHandler(
    IListaPreciosRepository repo,
    IMapper mapper)
    : IRequestHandler<GetPrecioItemQuery, ListaPreciosItemDto?>
{
    public async Task<ListaPreciosItemDto?> Handle(
        GetPrecioItemQuery request,
        CancellationToken ct)
    {
        var item = await repo.GetPrecioItemAsync(request.ListaId, request.ItemId, ct);
        return item is null ? null : mapper.Map<ListaPreciosItemDto>(item);
    }
}