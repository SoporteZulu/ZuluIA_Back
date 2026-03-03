using AutoMapper;
using MediatR;
using ZuluIA_Back.Application.Features.Cotizaciones.DTOs;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Cotizaciones.Queries;

public class GetCotizacionVigenteQueryHandler(
    ICotizacionMonedaRepository repo,
    IMapper mapper)
    : IRequestHandler<GetCotizacionVigenteQuery, CotizacionMonedaDto?>
{
    public async Task<CotizacionMonedaDto?> Handle(
        GetCotizacionVigenteQuery request,
        CancellationToken ct)
    {
        var cotizacion = await repo.GetVigenteAsync(request.MonedaId, request.Fecha, ct);
        return cotizacion is null ? null : mapper.Map<CotizacionMonedaDto>(cotizacion);
    }
}