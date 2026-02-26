using AutoMapper;
using MediatR;
using ZuluIA_Back.Application.Features.Contabilidad.DTOs;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Contabilidad.Queries;

public class GetAsientosPagedQueryHandler(
    IAsientoRepository repo,
    IMapper mapper)
    : IRequestHandler<GetAsientosPagedQuery, PagedResult<AsientoDto>>
{
    public async Task<PagedResult<AsientoDto>> Handle(GetAsientosPagedQuery request, CancellationToken ct)
    {
        var result = await repo.GetPagedAsync(
            request.Page,
            request.PageSize,
            request.EjercicioId,
            request.SucursalId,
            request.Estado,
            ct);

        var items = mapper.Map<IReadOnlyList<AsientoDto>>(result.Items);
        return new PagedResult<AsientoDto>(items, result.Page, result.PageSize, result.TotalCount);
    }
}