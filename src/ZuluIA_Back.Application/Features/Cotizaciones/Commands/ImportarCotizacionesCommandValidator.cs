using FluentValidation;

namespace ZuluIA_Back.Application.Features.Cotizaciones.Commands;

public class ImportarCotizacionesCommandValidator : AbstractValidator<ImportarCotizacionesCommand>
{
    public ImportarCotizacionesCommandValidator()
    {
        RuleFor(x => x.MonedaId)
            .GreaterThan(0).WithMessage("La moneda es obligatoria.");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Debe informar al menos una cotización.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.Cotizacion)
                .GreaterThan(0).WithMessage("La cotización debe ser mayor a 0.");

            item.RuleFor(x => x.Fecha)
                .NotEmpty().WithMessage("La fecha es obligatoria.")
                .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today).AddDays(1))
                .WithMessage("No se puede registrar una cotización futura.");
        });
    }
}