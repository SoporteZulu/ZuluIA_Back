using FluentValidation;

namespace ZuluIA_Back.Application.Features.Contratos.Commands;

public class RenovarContratosAutomaticamenteCommandValidator : AbstractValidator<RenovarContratosAutomaticamenteCommand>
{
    public RenovarContratosAutomaticamenteCommandValidator()
    {
        RuleFor(x => x.SucursalId).GreaterThan(0).When(x => x.SucursalId.HasValue);
        RuleFor(x => x.PorcentajeAjuste).GreaterThanOrEqualTo(-100m).When(x => x.PorcentajeAjuste.HasValue);
    }
}
