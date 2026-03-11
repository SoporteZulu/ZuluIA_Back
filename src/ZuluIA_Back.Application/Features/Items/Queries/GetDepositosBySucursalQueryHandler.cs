using AutoMapper;
using MediatR;
using ZuluIA_Back.Application.Features.Items.DTOs;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Items.Queries;

public class GetDepositosBySucursalQueryHandler(
    IDepositoRepository repo,
    IMapper mapper)
    : IRequestHandler<GetDepositosBySucursalQuery, IReadOnlyList<DepositoDto>>
{
    public async Task<IReadOnlyList<DepositoDto>> Handle(
        GetDepositosBySucursalQuery request,
        CancellationToken ct)
    {
        var depositos = await repo.GetActivosBySucursalAsync(request.SucursalId, ct);
        return mapper.Map<IReadOnlyList<DepositoDto>>(depositos);
    }
}