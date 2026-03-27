using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Comprobantes.Commands;

public record DesimputarComprobanteCommand(
    long ImputacionId,
    DateOnly Fecha,
    string? Motivo
) : IRequest<Result<long>>;
