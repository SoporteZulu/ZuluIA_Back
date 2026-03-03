using AutoMapper;
using MediatR;
using ZuluIA_Back.Application.Features.ListasPrecios.DTOs;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.ListasPrecios.Queries;

public class GetListasPreciosQueryHandler(
    IListaPreciosRepository repo,
    IMapper mapper)
    : IRequestHandler<GetListasPreciosQuery, IReadOnlyList<ListaPreciosDto>>
{
    public async Task<IReadOnlyList<ListaPreciosDto>> Handle(
        GetListasPreciosQuery request,
        CancellationToken ct)
    {
        var listas = await repo.GetActivasAsync(request.Fecha, ct);
        return mapper.Map<IReadOnlyList<ListaPreciosDto>>(listas);
    }
}