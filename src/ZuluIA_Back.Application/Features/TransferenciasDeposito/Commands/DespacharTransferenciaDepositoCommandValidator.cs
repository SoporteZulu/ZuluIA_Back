using FluentValidation;

namespace ZuluIA_Back.Application.Features.TransferenciasDeposito.Commands;

public class DespacharTransferenciaDepositoCommandValidator : AbstractValidator<DespacharTransferenciaDepositoCommand>
{
    public DespacharTransferenciaDepositoCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
