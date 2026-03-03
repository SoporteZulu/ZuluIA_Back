using AutoMapper;
using MediatR;
using ZuluIA_Back.Application.Features.PlanesPago.DTOs;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.PlanesPago.Queries;

public class GetPlanesPagoQueryHandler(
    IPlanPagoRepository repo,
    IMapper mapper)
    : IRequestHandler<GetPlanesPagoQuery, IReadOnlyList<PlanPagoDto>>
{
    public async Task<IReadOnlyList<PlanPagoDto>> Handle(
        GetPlanesPagoQuery request,
        CancellationToken ct)
    {
        var planes = request.SoloActivos
            ? await repo.GetActivosAsync(ct)
            : await repo.GetAllAsync(ct);

        return mapper.Map<IReadOnlyList<PlanPagoDto>>(planes);
    }
}