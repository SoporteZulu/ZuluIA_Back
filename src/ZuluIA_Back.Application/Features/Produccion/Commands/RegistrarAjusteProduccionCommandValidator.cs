using FluentValidation;

namespace ZuluIA_Back.Application.Features.Produccion.Commands;

public class RegistrarAjusteProduccionCommandValidator : AbstractValidator<RegistrarAjusteProduccionCommand>
{
    public RegistrarAjusteProduccionCommandValidator()
    {
        RuleFor(x => x.FormulaId).GreaterThan(0);
        RuleFor(x => x.DepositoOrigenId).GreaterThan(0);
        RuleFor(x => x.DepositoDestinoId).GreaterThan(0);
        RuleFor(x => x.Cantidad).GreaterThan(0);
    }
}
