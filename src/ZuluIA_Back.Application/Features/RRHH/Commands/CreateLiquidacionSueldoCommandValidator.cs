using FluentValidation;

namespace ZuluIA_Back.Application.Features.RRHH.Commands;

public class CreateLiquidacionSueldoCommandValidator : AbstractValidator<CreateLiquidacionSueldoCommand>
{
    public CreateLiquidacionSueldoCommandValidator()
    {
        RuleFor(x => x.EmpleadoId).GreaterThan(0);
        RuleFor(x => x.SucursalId).GreaterThan(0);
        RuleFor(x => x.Anio).InclusiveBetween(2000, 2100);
        RuleFor(x => x.Mes).InclusiveBetween(1, 12);
        RuleFor(x => x.SueldoBasico).GreaterThanOrEqualTo(0);
        RuleFor(x => x.TotalHaberes).GreaterThanOrEqualTo(0);
        RuleFor(x => x.TotalDescuentos).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MonedaId).GreaterThan(0);
    }
}
