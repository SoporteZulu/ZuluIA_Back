using MediatR;
using ZuluIA_Back.Application.Features.Terceros.DTOs;
using ZuluIA_Back.Domain.Interfaces;
using AutoMapper;

namespace ZuluIA_Back.Application.Features.Terceros.Queries;

/// <summary>
/// Devuelve la lista completa de proveedores activos para selectores y combos.
/// Equivalente al llenarComboProveedores() del VB6.
/// </summary>
public record GetProveedoresActivosQuery(long? SucursalId = null)
    : IRequest<IReadOnlyList<TerceroSelectorDto>>;

public class GetProveedoresActivosQueryHandler(
    ITerceroRepository repo,
    IMapper mapper)
    : IRequestHandler<GetProveedoresActivosQuery, IReadOnlyList<TerceroSelectorDto>>
{
    public async Task<IReadOnlyList<TerceroSelectorDto>> Handle(
        GetProveedoresActivosQuery request,
        CancellationToken ct)
    {
        var proveedores = await repo.GetProveedoresActivosAsync(request.SucursalId, ct);
        return mapper.Map<IReadOnlyList<TerceroSelectorDto>>(proveedores);
    }
}