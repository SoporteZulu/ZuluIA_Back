using FluentValidation;

namespace ZuluIA_Back.Application.Features.Terceros.Commands;

public class ReplaceTerceroVentanasCobranzaCommandValidator : AbstractValidator<ReplaceTerceroVentanasCobranzaCommand>
{
    public ReplaceTerceroVentanasCobranzaCommandValidator()
    {
        RuleFor(x => x.TerceroId)
            .GreaterThan(0);

        RuleForEach(x => x.Ventanas)
            .SetValidator(new ReplaceTerceroVentanaCobranzaItemValidator());

        RuleFor(x => x.Ventanas)
            .Must(items => items.Count(x => x.Principal) <= 1)
            .WithMessage("Solo puede marcarse una ventana de cobranza principal.");
    }
}

public class ReplaceTerceroVentanaCobranzaItemValidator : AbstractValidator<ReplaceTerceroVentanaCobranzaItem>
{
    public ReplaceTerceroVentanaCobranzaItemValidator()
    {
        RuleFor(x => x.Dia)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.Franja)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.Franja));

        RuleFor(x => x.Canal)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.Canal));

        RuleFor(x => x.Responsable)
            .MaximumLength(150)
            .When(x => !string.IsNullOrWhiteSpace(x.Responsable));

        RuleFor(x => x.Orden)
            .GreaterThanOrEqualTo(0)
            .When(x => x.Orden.HasValue);
    }
}
