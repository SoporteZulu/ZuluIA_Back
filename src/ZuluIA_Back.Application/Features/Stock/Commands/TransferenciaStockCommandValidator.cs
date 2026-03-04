using FluentValidation;

namespace ZuluIA_Back.Application.Features.Stock.Commands;

public class TransferenciaStockCommandValidator
    : AbstractValidator<TransferenciaStockCommand>
{
    public TransferenciaStockCommandValidator()
    {
        RuleFor(x => x.ItemId)
            .GreaterThan(0).WithMessage("El ítem es obligatorio.");

        RuleFor(x => x.DepositoOrigenId)
            .GreaterThan(0).WithMessage("El depósito de origen es obligatorio.");

        RuleFor(x => x.DepositoDestinoId)
            .GreaterThan(0).WithMessage("El depósito de destino es obligatorio.");

        RuleFor(x => x.DepositoDestinoId)
            .NotEqual(x => x.DepositoOrigenId)
            .WithMessage("El depósito de destino no puede ser igual al de origen.");

        RuleFor(x => x.Cantidad)
            .GreaterThan(0).WithMessage("La cantidad a transferir debe ser mayor a 0.");
    }
}