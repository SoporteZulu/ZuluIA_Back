using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Impuestos.Commands;

public record UnassignImpuestoTerceroCommand(long ImpuestoId, long TerceroId)
    : IRequest<Result>;
