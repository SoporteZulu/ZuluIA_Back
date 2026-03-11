using AutoMapper;
using MediatR;
using ZuluIA_Back.Application.Features.ListasPrecios.DTOs;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.ListasPrecios.Queries;

public class GetListaPreciosByIdQueryHandler(
    IListaPreciosRepository repo,
    IMapper mapper)
    : IRequestHandler<GetListaPreciosByIdQuery, ListaPreciosDetalleDto?>
{
    public async Task<ListaPreciosDetalleDto?> Handle(
        GetListaPreciosByIdQuery request,
        CancellationToken ct)
    {
        var lista = await repo.GetByIdConItemsAsync(request.Id, ct);
        return lista is null ? null : mapper.Map<ListaPreciosDetalleDto>(lista);
    }
}