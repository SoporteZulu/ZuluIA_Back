using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Facturacion.Commands;

public record UpdateTimbradoCommand(
    long Id,
    DateOnly FechaInicio,
    DateOnly FechaFin,
    int NroComprobanteDesde,
    int NroComprobanteHasta) : IRequest<Result>;