using FluentValidation;

namespace ZuluIA_Back.Application.Features.Ventas.Commands;

public class ReplaceRemitoAtributosCommandValidator : AbstractValidator<ReplaceRemitoAtributosCommand>
{
    public ReplaceRemitoAtributosCommandValidator()
    {
        RuleFor(x => x.ComprobanteId)
            .GreaterThan(0);

        RuleFor(x => x.Atributos)
            .NotNull();

        RuleForEach(x => x.Atributos)
            .ChildRules(attr =>
            {
                attr.RuleFor(x => x.Clave)
                    .NotEmpty()
                    .MaximumLength(100);

                attr.RuleFor(x => x.Valor)
                    .MaximumLength(500)
                    .When(x => !string.IsNullOrWhiteSpace(x.Valor));

                attr.RuleFor(x => x.TipoDato)
                    .MaximumLength(50)
                    .When(x => !string.IsNullOrWhiteSpace(x.TipoDato));
            });

        RuleFor(x => x.Atributos)
            .Must(NoTenerClavesDuplicadas)
            .WithMessage("No se pueden repetir claves de atributos de remito.");
    }

    private static bool NoTenerClavesDuplicadas(IReadOnlyList<RemitoAtributoInput> atributos)
    {
        var claves = atributos
            .Where(x => !string.IsNullOrWhiteSpace(x.Clave))
            .Select(x => x.Clave.Trim().ToUpperInvariant())
            .ToList();

        return claves.Count == claves.Distinct(StringComparer.Ordinal).Count();
    }
}
