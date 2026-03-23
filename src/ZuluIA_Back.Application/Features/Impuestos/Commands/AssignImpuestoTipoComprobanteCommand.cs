using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Impuestos.Commands;

public record AssignImpuestoTipoComprobanteCommand(
    long ImpuestoId,
    long TipoComprobanteId,
    int Orden) : IRequest<Result<long>>;
