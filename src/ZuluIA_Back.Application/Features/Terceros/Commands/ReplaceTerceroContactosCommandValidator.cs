using FluentValidation;

namespace ZuluIA_Back.Application.Features.Terceros.Commands;

public class ReplaceTerceroContactosCommandValidator : AbstractValidator<ReplaceTerceroContactosCommand>
{
    public ReplaceTerceroContactosCommandValidator()
    {
        RuleFor(x => x.TerceroId)
            .GreaterThan(0);

        RuleForEach(x => x.Contactos)
            .SetValidator(new ReplaceTerceroContactoItemValidator());

        RuleFor(x => x.Contactos)
            .Must(contactos => contactos.Count(c => c.Principal) <= 1)
            .WithMessage("Solo puede marcarse un contacto principal.");
    }
}

public class ReplaceTerceroContactoItemValidator : AbstractValidator<ReplaceTerceroContactoItem>
{
    public ReplaceTerceroContactoItemValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Cargo)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.Cargo));

        RuleFor(x => x.Email)
            .EmailAddress()
            .When(x => !string.IsNullOrWhiteSpace(x.Email))
            .MaximumLength(150)
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x.Telefono)
            .MaximumLength(50)
            .When(x => !string.IsNullOrWhiteSpace(x.Telefono));

        RuleFor(x => x.Sector)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.Sector));

        RuleFor(x => x.Orden)
            .GreaterThanOrEqualTo(0)
            .When(x => x.Orden.HasValue);
    }
}
