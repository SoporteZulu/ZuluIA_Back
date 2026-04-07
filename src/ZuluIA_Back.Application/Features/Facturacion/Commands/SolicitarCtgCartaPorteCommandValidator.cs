using FluentValidation;

namespace ZuluIA_Back.Application.Features.Facturacion.Commands;

public class SolicitarCtgCartaPorteCommandValidator : AbstractValidator<SolicitarCtgCartaPorteCommand>
{
    public SolicitarCtgCartaPorteCommandValidator()
    {
        RuleFor(x => x.CartaPorteId).GreaterThan(0);
    }
}
