using FluentValidation;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Application.Features.RRHH.Commands;

public class CambiarEstadoEmpleadoCommandValidator : AbstractValidator<CambiarEstadoEmpleadoCommand>
{
    public CambiarEstadoEmpleadoCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.FechaEgreso)
            .NotNull()
            .When(x => x.Estado == EstadoEmpleado.Inactivo)
            .WithMessage("La fecha de egreso es obligatoria para inactivar al empleado.");
    }
}
