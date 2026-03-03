using MediatR;
using ZuluIA_Back.Application.Features.Facturacion.DTOs;

namespace ZuluIA_Back.Application.Features.Facturacion.Queries;

public record GetPuntosFacturacionQuery(long SucursalId)
    : IRequest<IReadOnlyList<PuntoFacturacionListDto>>;