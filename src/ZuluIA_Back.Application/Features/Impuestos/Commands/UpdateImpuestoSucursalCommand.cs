using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Impuestos.Commands;

public record UpdateImpuestoSucursalCommand(
    long ImpuestoId,
    long AsignacionId,
    string? Descripcion,
    string? Observacion) : IRequest<Result<UpdateImpuestoSucursalResult>>;

public record UpdateImpuestoSucursalResult(
    long Id,
    string? Descripcion,
    string? Observacion);
