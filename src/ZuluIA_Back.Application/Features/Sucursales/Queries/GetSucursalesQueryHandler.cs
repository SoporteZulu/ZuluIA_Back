using AutoMapper;
using MediatR;
using ZuluIA_Back.Application.Features.Sucursales.DTOs;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Sucursales.Queries;

public class GetSucursalesQueryHandler(
    ISucursalRepository repo,
    IMapper mapper)
    : IRequestHandler<GetSucursalesQuery, IReadOnlyList<SucursalListDto>>
{
    public async Task<IReadOnlyList<SucursalListDto>> Handle(
        GetSucursalesQuery request,
        CancellationToken ct)
    {
        var sucursales = request.SoloActivas
            ? await repo.GetAllActivasAsync(ct)
            : await repo.GetAllAsync(ct);

        return mapper.Map<IReadOnlyList<SucursalListDto>>(sucursales);
    }
}