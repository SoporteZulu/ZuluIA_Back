using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.RRHH.Commands;

public record CreateLiquidacionSueldoCommand(
    long EmpleadoId,
    long SucursalId,
    int Anio,
    int Mes,
    decimal SueldoBasico,
    decimal TotalHaberes,
    decimal TotalDescuentos,
    long MonedaId,
    string? Observacion
) : IRequest<Result<long>>;