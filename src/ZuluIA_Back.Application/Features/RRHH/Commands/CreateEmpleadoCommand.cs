using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.RRHH.Commands;

public record CreateEmpleadoCommand(
    long TerceroId,
    long SucursalId,
    string Legajo,
    string Cargo,
    string? Area,
    DateOnly FechaIngreso,
    decimal SueldoBasico,
    long MonedaId
) : IRequest<Result<long>>;