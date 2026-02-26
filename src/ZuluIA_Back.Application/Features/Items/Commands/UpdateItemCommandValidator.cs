using FluentValidation;

namespace ZuluIA_Back.Application.Features.Items.Commands;

public class UpdateItemCommandValidator : AbstractValidator<UpdateItemCommand>
{
    public UpdateItemCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("El ID del ítem es inválido.");

        RuleFor(x => x.Descripcion)
            .NotEmpty().WithMessage("La descripción es obligatoria.")
            .MaximumLength(200).WithMessage("La descripción no puede superar los 200 caracteres.");

        RuleFor(x => x.PrecioCosto)
            .GreaterThanOrEqualTo(0).WithMessage("El precio de costo no puede ser negativo.");

        RuleFor(x => x.PrecioVenta)
            .GreaterThanOrEqualTo(0).WithMessage("El precio de venta no puede ser negativo.");

        RuleFor(x => x.StockMinimo)
            .GreaterThanOrEqualTo(0).WithMessage("El stock mínimo no puede ser negativo.");

        RuleFor(x => x.StockMaximo)
            .GreaterThanOrEqualTo(x => x.StockMinimo)
            .WithMessage("El stock máximo no puede ser menor al stock mínimo.")
            .When(x => x.StockMaximo.HasValue);
    }
}