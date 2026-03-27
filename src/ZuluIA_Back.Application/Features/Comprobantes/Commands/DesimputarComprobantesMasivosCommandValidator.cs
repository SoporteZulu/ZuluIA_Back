using FluentValidation;

namespace ZuluIA_Back.Application.Features.Comprobantes.Commands;

public class DesimputarComprobantesMasivosCommandValidator : AbstractValidator<DesimputarComprobantesMasivosCommand>
{
    public DesimputarComprobantesMasivosCommandValidator()
    {
        RuleFor(x => x.ImputacionIds)
            .NotEmpty()
            .WithMessage("Debe indicar al menos una imputación.");

        RuleForEach(x => x.ImputacionIds).GreaterThan(0);
    }
}
