using FluentValidation;

namespace ZuluIA_Back.Application.Features.Terceros.Commands;

public class ReplaceTerceroTransportesCommandValidator : AbstractValidator<ReplaceTerceroTransportesCommand>
{
    public ReplaceTerceroTransportesCommandValidator()
    {
        RuleFor(x => x.TerceroId)
            .GreaterThan(0);

        RuleForEach(x => x.Transportes)
            .SetValidator(new ReplaceTerceroTransporteItemValidator());

        RuleFor(x => x.Transportes)
            .Must(items => items.Count(x => x.Principal) <= 1)
            .WithMessage("Solo puede marcarse un transporte principal o por defecto.");
    }
}

public class ReplaceTerceroTransporteItemValidator : AbstractValidator<ReplaceTerceroTransporteItem>
{
    public ReplaceTerceroTransporteItemValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Servicio)
            .MaximumLength(150)
            .When(x => !string.IsNullOrWhiteSpace(x.Servicio));

        RuleFor(x => x.Zona)
            .MaximumLength(150)
            .When(x => !string.IsNullOrWhiteSpace(x.Zona));

        RuleFor(x => x.Frecuencia)
            .MaximumLength(150)
            .When(x => !string.IsNullOrWhiteSpace(x.Frecuencia));

        RuleFor(x => x.Observacion)
            .MaximumLength(500)
            .When(x => !string.IsNullOrWhiteSpace(x.Observacion));

        RuleFor(x => x.Orden)
            .GreaterThanOrEqualTo(0)
            .When(x => x.Orden.HasValue);
    }
}
