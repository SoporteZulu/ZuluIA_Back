using FluentValidation;

namespace ZuluIA_Back.Application.Features.Colegio.Commands;

public class DesactivarPlanGeneralColegioCommandValidator : AbstractValidator<DesactivarPlanGeneralColegioCommand>
{
    public DesactivarPlanGeneralColegioCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
