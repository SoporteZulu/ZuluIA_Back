using FluentValidation;

namespace ZuluIA_Back.Application.Features.Stock.Commands;

public class AjusteStockCommandValidator : AbstractValidator<AjusteStockCommand>
{
    public AjusteStockCommandValidator()
    {
        RuleFor(x => x.ItemId)
            .GreaterThan(0).WithMessage("El ítem es obligatorio.");

        RuleFor(x => x.DepositoId)
            .GreaterThan(0).WithMessage("El depósito es obligatorio.");

        RuleFor(x => x.NuevaCantidad)
            .GreaterThanOrEqualTo(0)
            .WithMessage("La cantidad no puede ser negativa.");
    }
}