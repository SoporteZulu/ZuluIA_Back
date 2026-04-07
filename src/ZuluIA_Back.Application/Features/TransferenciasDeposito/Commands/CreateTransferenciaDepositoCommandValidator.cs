using FluentValidation;

namespace ZuluIA_Back.Application.Features.TransferenciasDeposito.Commands;

public class CreateTransferenciaDepositoCommandValidator : AbstractValidator<CreateTransferenciaDepositoCommand>
{
    public CreateTransferenciaDepositoCommandValidator()
    {
        RuleFor(x => x.SucursalId).GreaterThan(0);
        RuleFor(x => x.DepositoOrigenId).GreaterThan(0);
        RuleFor(x => x.DepositoDestinoId).GreaterThan(0).NotEqual(x => x.DepositoOrigenId);
        RuleFor(x => x.Detalles).NotEmpty();
        RuleForEach(x => x.Detalles).ChildRules(c =>
        {
            c.RuleFor(x => x.ItemId).GreaterThan(0);
            c.RuleFor(x => x.Cantidad).GreaterThan(0);
        });
    }
}
