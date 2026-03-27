using MediatR;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Application.Features.RRHH.Commands;

public record CambiarEstadoEmpleadoCommand(
    long Id,
    EstadoEmpleado Estado,
    DateOnly? FechaEgreso = null) : IRequest<Result>;
