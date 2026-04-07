using FluentValidation;

namespace ZuluIA_Back.Application.Features.Facturacion.Commands;

public class ConsultarComprobanteAfipWsfeCommandValidator : AbstractValidator<ConsultarComprobanteAfipWsfeCommand>
{
    public ConsultarComprobanteAfipWsfeCommandValidator()
    {
        RuleFor(x => x.ComprobanteId).GreaterThan(0);
    }
}
