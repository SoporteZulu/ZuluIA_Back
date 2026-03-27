using MediatR;
using ZuluIA_Back.Application.Features.Recibos.DTOs;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Recibos.Queries;

public class GetRecibosPagedQueryHandler(IReciboRepository reciboRepo)
    : IRequestHandler<GetRecibosPagedQuery, PagedResult<ReciboListDto>>
{
    public async Task<PagedResult<ReciboListDto>> Handle(
        GetRecibosPagedQuery request, CancellationToken ct)
    {
        var paged = await reciboRepo.GetPagedAsync(
            request.Page, request.PageSize,
            request.SucursalId, request.TerceroId,
            request.Desde, request.Hasta, ct);

        var items = paged.Items.Select(r => new ReciboListDto
        {
            Id                  = r.Id,
            SucursalId          = r.SucursalId,
            TerceroId           = r.TerceroId,
            TerceroRazonSocial  = string.Empty,
            Fecha               = r.Fecha,
            Serie               = r.Serie,
            Numero              = r.Numero,
            Total               = r.Total,
            Estado              = r.Estado.ToString(),
            CobroId             = r.CobroId,
            CreatedAt           = r.CreatedAt
        }).ToList();

        return new PagedResult<ReciboListDto>(items, paged.TotalCount, request.Page, request.PageSize);
    }
}
