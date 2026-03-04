using FluentValidation;

namespace ZuluIA_Back.Application.Features.Stock.Commands;

public class StockInicialCommandValidator : AbstractValidator<StockInicialCommand>
{
    public StockInicialCommandValidator()
    {
        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Debe especificar al menos un ítem.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.ItemId)
                .GreaterThan(0).WithMessage("El ítem es obligatorio.");

            item.RuleFor(x => x.DepositoId)
                .GreaterThan(0).WithMessage("El depósito es obligatorio.");

            item.RuleFor(x => x.Cantidad)
                .GreaterThanOrEqualTo(0)
                .WithMessage("La cantidad no puede ser negativa.");
        });
    }
}