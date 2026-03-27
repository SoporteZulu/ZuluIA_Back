using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.RRHH.Commands;

public record UpdateEmpleadoCommand(
    long Id,
    string Cargo,
    string? Area,
    decimal SueldoBasico,
    long MonedaId) : IRequest<Result>;
