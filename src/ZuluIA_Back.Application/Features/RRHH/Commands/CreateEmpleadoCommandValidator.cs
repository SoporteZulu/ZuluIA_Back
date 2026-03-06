using FluentValidation;

namespace ZuluIA_Back.Application.Features.RRHH.Commands;

public class CreateEmpleadoCommandValidator
    : AbstractValidator<CreateEmpleadoCommand>
{
    public CreateEmpleadoCommandValidator()
    {
        RuleFor(x => x.TerceroId)
            .GreaterThan(0).WithMessage("El tercero es obligatorio.");

        RuleFor(x => x.SucursalId)
            .GreaterThan(0).WithMessage("La sucursal es obligatoria.");

        RuleFor(x => x.Legajo)
            .NotEmpty().WithMessage("El legajo es obligatorio.")
            .MaximumLength(30).WithMessage("El legajo no puede superar 30 caracteres.");

        RuleFor(x => x.Cargo)
            .NotEmpty().WithMessage("El cargo es obligatorio.");

        RuleFor(x => x.FechaIngreso)
            .NotEmpty().WithMessage("La fecha de ingreso es obligatoria.");

        RuleFor(x => x.SueldoBasico)
            .GreaterThanOrEqualTo(0)
            .WithMessage("El sueldo básico no puede ser negativo.");

        RuleFor(x => x.MonedaId)
            .GreaterThan(0).WithMessage("La moneda es obligatoria.");
    }
}