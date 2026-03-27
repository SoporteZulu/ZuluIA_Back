using FluentValidation;

namespace ZuluIA_Back.Application.Features.Items.Commands;

public class UpdateItemPreciosCommandValidator : AbstractValidator<UpdateItemPreciosCommand>
{
    public UpdateItemPreciosCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("El Id del ítem es obligatorio.");
        RuleFor(x => x.PrecioCosto).GreaterThanOrEqualTo(0).WithMessage("El precio de costo no puede ser negativo.");
        RuleFor(x => x.PrecioVenta).GreaterThanOrEqualTo(0).WithMessage("El precio de venta no puede ser negativo.");
    }
}
