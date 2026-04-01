using FluentValidation;
using ZuluIA_Back.Domain.Entities.Terceros;

namespace ZuluIA_Back.Application.Features.Terceros.Commands;

public class ReplaceTerceroDomiciliosCommandValidator : AbstractValidator<ReplaceTerceroDomiciliosCommand>
{
    public ReplaceTerceroDomiciliosCommandValidator()
    {
        RuleFor(x => x.TerceroId)
            .GreaterThan(0);

        RuleForEach(x => x.Domicilios)
            .SetValidator(new ReplaceTerceroDomicilioItemValidator());

        RuleFor(x => x.Domicilios)
            .Must(domicilios => domicilios.Count <= PersonaDomicilio.MaxCantidadPorPersona)
            .WithMessage($"Se permiten hasta {PersonaDomicilio.MaxCantidadPorPersona} domicilios por tercero.")
            .Must(domicilios => domicilios.Count(d => d.EsDefecto) <= 1)
            .WithMessage("Solo puede marcarse un domicilio por defecto.");
    }
}

public class ReplaceTerceroDomicilioItemValidator : AbstractValidator<ReplaceTerceroDomicilioItem>
{
    public ReplaceTerceroDomicilioItemValidator()
    {
        RuleFor(x => x.TipoDomicilioId)
            .GreaterThan(0)
            .When(x => x.TipoDomicilioId.HasValue);

        RuleFor(x => x.ProvinciaId)
            .GreaterThan(0)
            .When(x => x.ProvinciaId.HasValue);

        RuleFor(x => x.LocalidadId)
            .GreaterThan(0)
            .When(x => x.LocalidadId.HasValue);

        RuleFor(x => x.Calle)
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.Calle));

        RuleFor(x => x.Barrio)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.Barrio));

        RuleFor(x => x.CodigoPostal)
            .MaximumLength(20)
            .When(x => !string.IsNullOrWhiteSpace(x.CodigoPostal));

        RuleFor(x => x.Observacion)
            .MaximumLength(500)
            .When(x => !string.IsNullOrWhiteSpace(x.Observacion));

        RuleFor(x => x.Orden)
            .GreaterThanOrEqualTo(0)
            .When(x => x.Orden.HasValue);

        RuleFor(x => x)
            .Must(x =>
                x.LocalidadId.HasValue ||
                x.ProvinciaId.HasValue ||
                !string.IsNullOrWhiteSpace(x.Calle) ||
                !string.IsNullOrWhiteSpace(x.Barrio) ||
                !string.IsNullOrWhiteSpace(x.CodigoPostal) ||
                !string.IsNullOrWhiteSpace(x.Observacion))
            .WithMessage("Debe informar al menos un dato del domicilio.");
    }
}
