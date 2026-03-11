using AutoMapper;
using MediatR;
using ZuluIA_Back.Application.Features.Cajas.DTOs;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Cajas.Queries;

public class GetCajasBySucursalQueryHandler(
    ICajaRepository repo,
    IMapper mapper)
    : IRequestHandler<GetCajasBySucursalQuery, IReadOnlyList<CajaListDto>>
{
    public async Task<IReadOnlyList<CajaListDto>> Handle(
        GetCajasBySucursalQuery request,
        CancellationToken ct)
    {
        var cajas = await repo.GetActivasBySucursalAsync(request.SucursalId, ct);
        return mapper.Map<IReadOnlyList<CajaListDto>>(cajas);
    }
}