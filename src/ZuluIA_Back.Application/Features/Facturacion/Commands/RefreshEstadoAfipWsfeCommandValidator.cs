using FluentValidation;

namespace ZuluIA_Back.Application.Features.Facturacion.Commands;

public class RefreshEstadoAfipWsfeCommandValidator : AbstractValidator<RefreshEstadoAfipWsfeCommand>
{
    public RefreshEstadoAfipWsfeCommandValidator()
    {
        RuleFor(x => x.ComprobanteId).GreaterThan(0);
    }
}
