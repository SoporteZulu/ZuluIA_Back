using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Impuestos.Commands;

public record AssignImpuestoSucursalCommand(
    long ImpuestoId,
    long SucursalId,
    string? Descripcion,
    string? Observacion) : IRequest<Result<long>>;
