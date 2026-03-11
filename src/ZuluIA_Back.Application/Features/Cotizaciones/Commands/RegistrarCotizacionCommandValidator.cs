using FluentValidation;

namespace ZuluIA_Back.Application.Features.Cotizaciones.Commands;

public class RegistrarCotizacionCommandValidator : AbstractValidator<RegistrarCotizacionCommand>
{
    public RegistrarCotizacionCommandValidator()
    {
        RuleFor(x => x.MonedaId)
            .GreaterThan(0).WithMessage("La moneda es obligatoria.");

        RuleFor(x => x.Cotizacion)
            .GreaterThan(0).WithMessage("La cotización debe ser mayor a 0.");

        RuleFor(x => x.Fecha)
            .NotEmpty().WithMessage("La fecha es obligatoria.")
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today).AddDays(1))
            .WithMessage("No se puede registrar una cotización futura.");
    }
}