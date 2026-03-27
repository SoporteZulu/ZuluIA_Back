using FluentValidation;

namespace ZuluIA_Back.Application.Features.RRHH.Commands;

public class UpdateLiquidacionSueldoCommandValidator : AbstractValidator<UpdateLiquidacionSueldoCommand>
{
    public UpdateLiquidacionSueldoCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.SueldoBasico).GreaterThanOrEqualTo(0);
        RuleFor(x => x.TotalHaberes).GreaterThanOrEqualTo(0);
        RuleFor(x => x.TotalDescuentos).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MonedaId).GreaterThan(0);
    }
}
