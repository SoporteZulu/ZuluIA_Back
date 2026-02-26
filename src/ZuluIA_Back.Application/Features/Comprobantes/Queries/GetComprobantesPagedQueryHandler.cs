using AutoMapper;
using MediatR;
using ZuluIA_Back.Application.Features.Comprobantes.DTOs;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Comprobantes.Queries;

public class GetComprobantesPagedQueryHandler(
    IComprobanteRepository repo,
    IMapper mapper)
    : IRequestHandler<GetComprobantesPagedQuery, PagedResult<ComprobanteListDto>>
{
    public async Task<PagedResult<ComprobanteListDto>> Handle(GetComprobantesPagedQuery request, CancellationToken ct)
    {
        var result = await repo.GetPagedAsync(
            request.Page,
            request.PageSize,
            request.SucursalId,
            request.TerceroId,
            request.TipoId,
            request.Estado,
            request.Desde,
            request.Hasta,
            ct);

        var items = mapper.Map<IReadOnlyList<ComprobanteListDto>>(result.Items);
        return new PagedResult<ComprobanteListDto>(items, result.Page, result.PageSize, result.TotalCount);
    }
}