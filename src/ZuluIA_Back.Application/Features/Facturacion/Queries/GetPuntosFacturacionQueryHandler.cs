using AutoMapper;
using MediatR;
using ZuluIA_Back.Application.Features.Facturacion.DTOs;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Facturacion.Queries;

public class GetPuntosFacturacionQueryHandler(
    IPuntoFacturacionRepository repo,
    IMapper mapper)
    : IRequestHandler<GetPuntosFacturacionQuery, IReadOnlyList<PuntoFacturacionListDto>>
{
    public async Task<IReadOnlyList<PuntoFacturacionListDto>> Handle(
        GetPuntosFacturacionQuery request,
        CancellationToken ct)
    {
        var puntos = await repo.GetActivosBySucursalAsync(request.SucursalId, ct);
        return mapper.Map<IReadOnlyList<PuntoFacturacionListDto>>(puntos);
    }
}