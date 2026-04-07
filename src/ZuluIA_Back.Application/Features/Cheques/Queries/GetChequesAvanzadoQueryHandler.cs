using AutoMapper;
using MediatR;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Cheques.DTOs;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Enums;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Cheques.Queries;

public class GetChequesAvanzadoQueryHandler(
    IChequeRepository repo,
    IMapper mapper,
    IApplicationDbContext db)
    : IRequestHandler<GetChequesAvanzadoQuery, PagedResult<ChequeDto>>
{
    public async Task<PagedResult<ChequeDto>> Handle(
        GetChequesAvanzadoQuery request,
        CancellationToken ct)
    {
        var effectiveEstado = !request.IncluirAnulados && request.Estado is null
            ? null
            : request.Estado;

        var baseHandler = new GetChequesPagedQueryHandler(repo, mapper, db);
        var result = await baseHandler.Handle(
            new GetChequesPagedQuery(
                request.Page,
                request.PageSize,
                request.CajaId,
                request.TerceroId,
                effectiveEstado,
                request.Tipo,
                request.EsALaOrden,
                request.EsCruzado,
                request.Banco,
                request.NroCheque,
                request.Titular,
                request.FechaEmisionDesde,
                request.FechaEmisionHasta),
            ct);

        if (request.IncluirAnulados || request.Estado == EstadoCheque.Anulado)
            return result;

        var filtrados = result.Items
            .Where(x => !string.Equals(x.Estado, EstadoCheque.Anulado.ToString().ToUpperInvariant(), StringComparison.OrdinalIgnoreCase))
            .ToList()
            .AsReadOnly();

        return new PagedResult<ChequeDto>(filtrados, result.Page, result.PageSize, result.TotalCount);
    }
}
