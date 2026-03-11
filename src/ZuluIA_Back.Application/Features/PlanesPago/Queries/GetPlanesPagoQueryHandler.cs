using AutoMapper;
using MediatR;
using ZuluIA_Back.Application.Features.PlanesPago.DTOs;
using ZuluIA_Back.Domain.Entities.Ventas;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.PlanesPago.Queries
{
    public class GetPlanesPagoQueryHandler : IRequestHandler<GetPlanesPagoQuery, IReadOnlyList<PlanPagoDto>>
    {
        private readonly IMapper mapper;
        private readonly IPlanPagoRepository repo;

        public GetPlanesPagoQueryHandler(IMapper mapper, IPlanPagoRepository repo)
        {
            this.mapper = mapper;
            this.repo = repo;
        }

        public async Task<IReadOnlyList<PlanPagoDto>> Handle(
            GetPlanesPagoQuery request,
            CancellationToken ct)
        {
            var planes = request.SoloActivos
                ? (IReadOnlyList<PlanPago>)await repo.GetActivosAsync(ct)
                : (IReadOnlyList<PlanPago>)await repo.GetAllAsync(ct);

            return mapper.Map<IReadOnlyList<PlanPagoDto>>(planes);
        }
    }
}
