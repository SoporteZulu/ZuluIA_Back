using FluentValidation;

namespace ZuluIA_Back.Application.Features.Facturacion.Commands;

public class AutorizarComprobanteAfipWsfeCommandValidator : AbstractValidator<AutorizarComprobanteAfipWsfeCommand>
{
    public AutorizarComprobanteAfipWsfeCommandValidator()
    {
        RuleFor(x => x.ComprobanteId).GreaterThan(0);
    }
}
