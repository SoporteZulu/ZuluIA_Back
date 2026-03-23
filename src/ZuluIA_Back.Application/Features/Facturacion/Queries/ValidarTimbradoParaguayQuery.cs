using MediatR;
using ZuluIA_Back.Application.Features.Facturacion.DTOs;

namespace ZuluIA_Back.Application.Features.Facturacion.Queries;

public record ValidarTimbradoParaguayQuery(
    long SucursalId,
    long? PuntoFacturacionId,
    long TipoComprobanteId,
    DateOnly Fecha,
    long NumeroComprobante)
    : IRequest<ValidacionTimbradoParaguayDto>;