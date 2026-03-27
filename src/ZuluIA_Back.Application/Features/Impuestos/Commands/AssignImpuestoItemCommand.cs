using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Impuestos.Commands;

public record AssignImpuestoItemCommand(
    long ImpuestoId,
    long ItemId,
    string? Descripcion,
    string? Observacion) : IRequest<Result<long>>;
