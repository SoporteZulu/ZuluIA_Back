using FluentValidation;

namespace ZuluIA_Back.Application.Features.Facturacion.Commands;

public class AnularCartaPorteWorkflowCommandValidator : AbstractValidator<AnularCartaPorteWorkflowCommand>
{
    public AnularCartaPorteWorkflowCommandValidator()
    {
        RuleFor(x => x.CartaPorteId).GreaterThan(0);
    }
}
