using FluentValidation;

namespace ZuluIA_Back.Application.Features.Comprobantes.Commands;

public class DesimputarComprobanteCommandValidator : AbstractValidator<DesimputarComprobanteCommand>
{
    public DesimputarComprobanteCommandValidator()
    {
        RuleFor(x => x.ImputacionId).GreaterThan(0);
    }
}
