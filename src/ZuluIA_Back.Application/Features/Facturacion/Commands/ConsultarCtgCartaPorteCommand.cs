using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Facturacion.Commands;

public record ConsultarCtgCartaPorteCommand(
    long CartaPorteId,
    DateOnly FechaConsulta,
    string? NroCtg,
    string? Error,
    string? Observacion
) : IRequest<Result>;
