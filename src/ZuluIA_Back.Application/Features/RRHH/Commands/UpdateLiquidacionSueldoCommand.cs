using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.RRHH.Commands;

public record UpdateLiquidacionSueldoCommand(
    long Id,
    decimal SueldoBasico,
    decimal TotalHaberes,
    decimal TotalDescuentos,
    long MonedaId,
    string? Observacion) : IRequest<Result>;
