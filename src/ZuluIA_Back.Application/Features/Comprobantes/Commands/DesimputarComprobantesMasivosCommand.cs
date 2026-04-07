using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Comprobantes.Commands;

public record DesimputarComprobantesMasivosCommand(
    DateOnly Fecha,
    string? Motivo,
    IReadOnlyList<long> ImputacionIds
) : IRequest<Result<IReadOnlyList<long>>>;
