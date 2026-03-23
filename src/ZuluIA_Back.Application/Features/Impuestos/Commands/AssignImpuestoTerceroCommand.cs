using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Impuestos.Commands;

public record AssignImpuestoTerceroCommand(
    long ImpuestoId,
    long TerceroId,
    string? Descripcion,
    string? Observacion) : IRequest<Result<long>>;
