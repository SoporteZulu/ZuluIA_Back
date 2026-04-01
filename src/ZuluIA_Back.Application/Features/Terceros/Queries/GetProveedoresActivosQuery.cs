using AutoMapper;
using MediatR;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Terceros.DTOs;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Terceros.Queries;

/// <summary>
/// Devuelve la lista completa de proveedores activos para selectores y combos.
/// Equivalente al llenarComboProveedores() del VB6.
/// </summary>
public record GetProveedoresActivosQuery(long? SucursalId = null)
    : IRequest<IReadOnlyList<TerceroSelectorDto>>;

public class GetProveedoresActivosQueryHandler(
    ITerceroRepository repo,
    IApplicationDbContext db,
    IMapper mapper)
    : IRequestHandler<GetProveedoresActivosQuery, IReadOnlyList<TerceroSelectorDto>>
{
    public async Task<IReadOnlyList<TerceroSelectorDto>> Handle(
        GetProveedoresActivosQuery request,
        CancellationToken ct)
    {
        var proveedores = await repo.GetProveedoresActivosAsync(request.SucursalId, ct);
        var dtos = mapper.Map<List<TerceroSelectorDto>>(proveedores);
        await TerceroSelectorReadModelLoader.LoadUbicacionAsync(db, proveedores, dtos, ct);
        return dtos;
    }
}