using FluentValidation;

namespace ZuluIA_Back.Application.Features.TransferenciasDeposito.Commands;

public class AnularTransferenciaDepositoCommandValidator : AbstractValidator<AnularTransferenciaDepositoCommand>
{
    public AnularTransferenciaDepositoCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
