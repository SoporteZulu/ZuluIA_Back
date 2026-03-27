using FluentValidation;

namespace ZuluIA_Back.Application.Features.Facturacion.Commands;

public class ConfirmarCartaPorteCommandValidator : AbstractValidator<ConfirmarCartaPorteCommand>
{
    public ConfirmarCartaPorteCommandValidator()
    {
        RuleFor(x => x.CartaPorteId).GreaterThan(0);
    }
}
