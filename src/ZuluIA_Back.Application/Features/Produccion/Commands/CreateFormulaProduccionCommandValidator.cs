using FluentValidation;

namespace ZuluIA_Back.Application.Features.Produccion.Commands;

public class CreateFormulaProduccionCommandValidator
    : AbstractValidator<CreateFormulaProduccionCommand>
{
    public CreateFormulaProduccionCommandValidator()
    {
        RuleFor(x => x.Codigo)
            .NotEmpty().WithMessage("El código es obligatorio.")
            .MaximumLength(50).WithMessage("El código no puede superar 50 caracteres.");

        RuleFor(x => x.Descripcion)
            .NotEmpty().WithMessage("La descripción es obligatoria.");

        RuleFor(x => x.ItemResultadoId)
            .GreaterThan(0).WithMessage("El ítem resultado es obligatorio.");

        RuleFor(x => x.CantidadResultado)
            .GreaterThan(0).WithMessage("La cantidad resultado debe ser mayor a 0.");

        RuleFor(x => x.Ingredientes)
            .NotEmpty().WithMessage("La fórmula debe tener al menos un ingrediente.");

        RuleForEach(x => x.Ingredientes).ChildRules(i =>
        {
            i.RuleFor(x => x.ItemId)
             .GreaterThan(0).WithMessage("El ítem del ingrediente es obligatorio.");

            i.RuleFor(x => x.Cantidad)
             .GreaterThan(0).WithMessage("La cantidad del ingrediente debe ser mayor a 0.");
        });
    }
}