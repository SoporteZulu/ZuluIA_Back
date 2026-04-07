using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Facturacion.Commands;

public record ConfirmarCartaPorteCommand(
    long CartaPorteId,
    DateOnly Fecha,
    string? Observacion
) : IRequest<Result>;
