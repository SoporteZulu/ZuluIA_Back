using FluentValidation;

namespace ZuluIA_Back.Application.Features.Items.Commands;

public class UpdateItemPorcentajeGananciaCommandValidator : AbstractValidator<UpdateItemPorcentajeGananciaCommand>
{
    public UpdateItemPorcentajeGananciaCommandValidator()
    {
        RuleFor(x => x.ItemId)
            .GreaterThan(0).WithMessage("El ID del ítem es inválido.");

        RuleFor(x => x.PorcentajeGanancia)
            .GreaterThanOrEqualTo(0).WithMessage("El porcentaje de ganancia no puede ser negativo.")
            .When(x => x.PorcentajeGanancia.HasValue);
    }
}
