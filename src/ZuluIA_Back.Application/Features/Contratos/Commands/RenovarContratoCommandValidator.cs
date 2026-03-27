using FluentValidation;

namespace ZuluIA_Back.Application.Features.Contratos.Commands;

public class RenovarContratoCommandValidator : AbstractValidator<RenovarContratoCommand>
{
    public RenovarContratoCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.NuevoImporte).GreaterThanOrEqualTo(0).When(x => x.NuevoImporte.HasValue);
    }
}
