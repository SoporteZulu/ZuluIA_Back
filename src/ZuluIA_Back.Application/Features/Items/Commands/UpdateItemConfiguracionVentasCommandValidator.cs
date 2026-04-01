using FluentValidation;

namespace ZuluIA_Back.Application.Features.Items.Commands;

public class UpdateItemConfiguracionVentasCommandValidator : AbstractValidator<UpdateItemConfiguracionVentasCommand>
{
    public UpdateItemConfiguracionVentasCommandValidator()
    {
        RuleFor(x => x.ItemId)
            .GreaterThan(0).WithMessage("El ID del ítem es inválido.");

        RuleFor(x => x.PorcentajeMaximoDescuento)
            .InclusiveBetween(0, 100).WithMessage("El porcentaje máximo de descuento debe estar entre 0 y 100.")
            .When(x => x.PorcentajeMaximoDescuento.HasValue);
    }
}
