using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Facturacion.Commands;

public record AnularCartaPorteWorkflowCommand(
    long CartaPorteId,
    DateOnly Fecha,
    string? Observacion
) : IRequest<Result>;
