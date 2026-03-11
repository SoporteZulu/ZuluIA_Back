using AutoMapper;
using MediatR;
using ZuluIA_Back.Application.Features.Sucursales.DTOs;
using ZuluIA_Back.Domain.Entities.Sucursales;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Sucursales.Queries
{
    public class GetSucursalesQueryHandler : IRequestHandler<GetSucursalesQuery, IReadOnlyList<SucursalListDto>>
    {
        private readonly IMapper mapper;
        private readonly ISucursalRepository repo;

        public GetSucursalesQueryHandler(IMapper mapper, ISucursalRepository repo)
        {
            this.mapper = mapper;
            this.repo = repo;
        }

        public async Task<IReadOnlyList<SucursalListDto>> Handle(
            GetSucursalesQuery request,
            CancellationToken ct)
        {
            var sucursales = request.SoloActivas
                ? (IReadOnlyList<Sucursal>)await repo.GetAllActivasAsync(ct)
                : (IReadOnlyList<Sucursal>)await repo.GetAllAsync(ct);

            return mapper.Map<IReadOnlyList<SucursalListDto>>(sucursales);
        }
    }
}
