using AutoMapper;
using MediatR;
using ZuluIA_Back.Application.Features.Cotizaciones.DTOs;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Cotizaciones.Queries;

public class GetHistoricoCotizacionesQueryHandler(
    ICotizacionMonedaRepository repo,
    IMapper mapper)
    : IRequestHandler<GetHistoricoCotizacionesQuery, IReadOnlyList<CotizacionMonedaDto>>
{
    public async Task<IReadOnlyList<CotizacionMonedaDto>> Handle(
        GetHistoricoCotizacionesQuery request,
        CancellationToken ct)
    {
        var historico = await repo.GetHistoricoAsync(
            request.MonedaId, request.Desde, request.Hasta, ct);
        return mapper.Map<IReadOnlyList<CotizacionMonedaDto>>(historico);
    }
}