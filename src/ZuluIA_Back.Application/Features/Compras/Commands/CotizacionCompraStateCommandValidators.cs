using FluentValidation;

namespace ZuluIA_Back.Application.Features.Compras.Commands;

public class AceptarCotizacionCompraCommandValidator : AbstractValidator<AceptarCotizacionCompraCommand>
{
    public AceptarCotizacionCompraCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

public class RechazarCotizacionCompraCommandValidator : AbstractValidator<RechazarCotizacionCompraCommand>
{
    public RechazarCotizacionCompraCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}