using FluentValidation;

namespace ZuluIA_Back.Application.Features.RRHH.Commands;

public class CreateLiquidacionSueldoCommandValidator : AbstractValidator<CreateLiquidacionSueldoCommand>
{
    public CreateLiquidacionSueldoCommandValidator()
    {
        RuleFor(x => x.EmpleadoId).GreaterThan(0).WithMessage("El empleado es obligatorio.");
        RuleFor(x => x.SucursalId).GreaterThan(0).WithMessage("La sucursal es obligatoria.");
        RuleFor(x => x.MonedaId).GreaterThan(0).WithMessage("La moneda es obligatoria.");
        RuleFor(x => x.Anio).InclusiveBetween(2000, 2100).WithMessage("El año debe estar entre 2000 y 2100.");
        RuleFor(x => x.Mes).InclusiveBetween(1, 12).WithMessage("El mes debe estar entre 1 y 12.");
        RuleFor(x => x.SueldoBasico).GreaterThanOrEqualTo(0).WithMessage("El sueldo básico no puede ser negativo.");
        RuleFor(x => x.TotalHaberes).GreaterThanOrEqualTo(0).WithMessage("El total de haberes no puede ser negativo.");
        RuleFor(x => x.TotalDescuentos).GreaterThanOrEqualTo(0).WithMessage("El total de descuentos no puede ser negativo.");
    }
}
