using MediatR;
using ZuluIA_Back.Application.Features.Facturacion.DTOs;

namespace ZuluIA_Back.Application.Features.Facturacion.Queries;

public record GetProximoNumeroQuery(
    long PuntoFacturacionId,
    long TipoComprobanteId)
    : IRequest<ProximoNumeroDto>;