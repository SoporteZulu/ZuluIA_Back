using FluentValidation;

namespace ZuluIA_Back.Application.Features.ListasPrecios.Commands;

public class UpsertItemEnListaCommandValidator : AbstractValidator<UpsertItemEnListaCommand>
{
    public UpsertItemEnListaCommandValidator()
    {
        RuleFor(x => x.ListaId)
            .GreaterThan(0).WithMessage("El ID de la lista es inválido.");

        RuleFor(x => x.ItemId)
            .GreaterThan(0).WithMessage("El ID del ítem es inválido.");

        RuleFor(x => x.Precio)
            .GreaterThanOrEqualTo(0).WithMessage("El precio no puede ser negativo.");

        RuleFor(x => x.DescuentoPct)
            .InclusiveBetween(0, 100).WithMessage("El descuento debe estar entre 0 y 100.");
    }
}