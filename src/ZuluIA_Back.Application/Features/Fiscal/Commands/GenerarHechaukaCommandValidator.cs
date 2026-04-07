using FluentValidation;

namespace ZuluIA_Back.Application.Features.Fiscal.Commands;

public class GenerarHechaukaCommandValidator : AbstractValidator<GenerarHechaukaCommand>
{
    public GenerarHechaukaCommandValidator()
    {
        RuleFor(x => x.SucursalId).GreaterThan(0);
        RuleFor(x => x.Hasta).GreaterThanOrEqualTo(x => x.Desde);
    }
}
