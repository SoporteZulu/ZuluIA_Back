using AutoMapper;
using MediatR;
using ZuluIA_Back.Application.Features.Cheques.DTOs;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Cheques.Queries;

public class GetChequesPagedQueryHandler(
    IChequeRepository repo,
    IMapper mapper)
    : IRequestHandler<GetChequesPagedQuery, PagedResult<ChequeDto>>
{
    public async Task<PagedResult<ChequeDto>> Handle(
        GetChequesPagedQuery request,
        CancellationToken ct)
    {
        var result = await repo.GetPagedAsync(
            request.Page, request.PageSize,
            request.CajaId, request.TerceroId,
            request.Estado, request.Desde, request.Hasta,
            ct);

        var items = mapper.Map<IReadOnlyList<ChequeDto>>(result.Items);
        return new PagedResult<ChequeDto>(items, result.Page, result.PageSize, result.TotalCount);
    }
}