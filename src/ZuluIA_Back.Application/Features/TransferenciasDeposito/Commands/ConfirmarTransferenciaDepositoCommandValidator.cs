using FluentValidation;

namespace ZuluIA_Back.Application.Features.TransferenciasDeposito.Commands;

public class ConfirmarTransferenciaDepositoCommandValidator : AbstractValidator<ConfirmarTransferenciaDepositoCommand>
{
    public ConfirmarTransferenciaDepositoCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
