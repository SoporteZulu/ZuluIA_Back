using FluentValidation;

namespace ZuluIA_Back.Application.Features.OrdenesPreparacion.Commands;

public class DespacharOrdenPreparacionCommandValidator : AbstractValidator<DespacharOrdenPreparacionCommand>
{
    public DespacharOrdenPreparacionCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.DepositoDestinoId).GreaterThan(0);
    }
}
