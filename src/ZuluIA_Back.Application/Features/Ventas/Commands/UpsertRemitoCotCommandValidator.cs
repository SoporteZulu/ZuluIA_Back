using FluentValidation;

namespace ZuluIA_Back.Application.Features.Ventas.Commands;

public class UpsertRemitoCotCommandValidator : AbstractValidator<UpsertRemitoCotCommand>
{
    public UpsertRemitoCotCommandValidator()
    {
        RuleFor(x => x.ComprobanteId)
            .GreaterThan(0);

        RuleFor(x => x.Numero)
            .NotEmpty()
            .MinimumLength(6)
            .MaximumLength(50);

        RuleFor(x => x.FechaVigencia)
            .Must(x => x != default)
            .WithMessage("La fecha de vigencia del COT es obligatoria.");

        RuleFor(x => x.Descripcion)
            .MaximumLength(500)
            .When(x => !string.IsNullOrWhiteSpace(x.Descripcion));
    }
}
