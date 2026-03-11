using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Comprobantes.Commands;

public record ImputarComprobanteCommand(
    long ComprobanteOrigenId,
    long ComprobanteDestinoId,
    decimal Importe,
    DateOnly Fecha
) : IRequest<Result<long>>;