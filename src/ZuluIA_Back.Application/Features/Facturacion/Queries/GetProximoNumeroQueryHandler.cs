using MediatR;
using ZuluIA_Back.Application.Features.Facturacion.DTOs;
using ZuluIA_Back.Domain.Services;

namespace ZuluIA_Back.Application.Features.Facturacion.Queries;

public class GetProximoNumeroQueryHandler(NumeracionComprobanteService numeracion)
    : IRequestHandler<GetProximoNumeroQuery, ProximoNumeroDto>
{
    public async Task<ProximoNumeroDto> Handle(
        GetProximoNumeroQuery request,
        CancellationToken ct)
    {
        var (prefijo, numero) = await numeracion.ObtenerProximoNumeroAsync(
            request.PuntoFacturacionId,
            request.TipoComprobanteId,
            ct);

        return new ProximoNumeroDto
        {
            PuntoFacturacionId = request.PuntoFacturacionId,
            TipoComprobanteId  = request.TipoComprobanteId,
            Prefijo            = prefijo,
            ProximoNumero      = numero,
            NumeroFormateado   = $"{prefijo:D4}-{numero:D8}"
        };
    }
}