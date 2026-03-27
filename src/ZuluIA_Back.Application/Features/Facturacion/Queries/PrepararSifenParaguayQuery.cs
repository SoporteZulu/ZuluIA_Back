using MediatR;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Facturacion.DTOs;

namespace ZuluIA_Back.Application.Features.Facturacion.Queries;

public record PrepararSifenParaguayQuery(long ComprobanteId)
    : IRequest<PreparacionSifenParaguayDto?>;

public class PrepararSifenParaguayQueryHandler(
    IParaguaySifenComprobanteService sifenComprobanteService)
    : IRequestHandler<PrepararSifenParaguayQuery, PreparacionSifenParaguayDto?>
{
    public Task<PreparacionSifenParaguayDto?> Handle(PrepararSifenParaguayQuery request, CancellationToken ct)
        => sifenComprobanteService.PrepararAsync(request.ComprobanteId, ct);
}