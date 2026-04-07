using FluentValidation;

namespace ZuluIA_Back.Application.Features.ListasPrecios.Commands;

public class CreateListaPreciosCommandValidator : AbstractValidator<CreateListaPreciosCommand>
{
    public CreateListaPreciosCommandValidator()
    {
        RuleFor(x => x.Descripcion)
            .NotEmpty().WithMessage("La descripción es obligatoria.")
            .MaximumLength(200);

        RuleFor(x => x.MonedaId)
            .GreaterThan(0).WithMessage("La moneda es obligatoria.");

        RuleFor(x => x.ListaPadreId)
            .GreaterThan(0)
            .When(x => x.ListaPadreId.HasValue)
            .WithMessage("La lista padre es inválida.");

        RuleFor(x => x.Prioridad)
            .GreaterThanOrEqualTo(0)
            .WithMessage("La prioridad no puede ser negativa.");

        RuleFor(x => x.Observaciones)
            .MaximumLength(500);

        RuleFor(x => x.VigenciaHasta)
            .GreaterThanOrEqualTo(x => x.VigenciaDesde)
            .When(x => x.VigenciaDesde.HasValue && x.VigenciaHasta.HasValue)
            .WithMessage("La fecha de vigencia hasta no puede ser anterior a la de inicio.");
    }
}