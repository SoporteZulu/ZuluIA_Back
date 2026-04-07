using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Finanzas.Commands;

public record CreateSeguimientoOrdenPagoCommand(
    long PagoId,
    long SucursalId,
    DateOnly Fecha,
    string Estado,
    string? Observacion,
    long? UsuarioId) : IRequest<Result<long>>;

public record UpdateSeguimientoOrdenPagoObservacionCommand(
    long Id,
    string? Observacion,
    long? UsuarioId) : IRequest<Result<UpdateSeguimientoOrdenPagoObservacionResult>>;

public record UpdateSeguimientoOrdenPagoObservacionResult(long Id, string? Observacion);
