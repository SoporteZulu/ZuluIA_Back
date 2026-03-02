using AutoMapper;
using MediatR;
using ZuluIA_Back.Application.Features.Terceros.DTOs;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Terceros.Queries;

/// <summary>
/// Devuelve la lista completa de clientes activos para selectores y combos.
/// Equivalente al llenarComboClientes() del VB6.
/// </summary>
public record GetClientesActivosQuery(long? SucursalId = null)
    : IRequest<IReadOnlyList<TerceroSelectorDto>>;

public class GetClientesActivosQueryHandler(
    ITerceroRepository repo,
    IMapper mapper)
    : IRequestHandler<GetClientesActivosQuery, IReadOnlyList<TerceroSelectorDto>>
{
    public async Task<IReadOnlyList<TerceroSelectorDto>> Handle(
        GetClientesActivosQuery request,
        CancellationToken ct)
    {
        var clientes = await repo.GetClientesActivosAsync(request.SucursalId, ct);
        return mapper.Map<IReadOnlyList<TerceroSelectorDto>>(clientes);
    }
}