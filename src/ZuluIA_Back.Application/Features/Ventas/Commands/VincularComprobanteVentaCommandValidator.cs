using FluentValidation;

namespace ZuluIA_Back.Application.Features.Ventas.Commands;

public class VincularComprobanteVentaCommandValidator : AbstractValidator<VincularComprobanteVentaCommand>
{
    public VincularComprobanteVentaCommandValidator()
    {
        RuleFor(x => x.ComprobanteOrigenId).GreaterThan(0);
        RuleFor(x => x.ComprobanteDestinoId).GreaterThan(0);
        RuleFor(x => x)
            .Must(x => x.ComprobanteOrigenId != x.ComprobanteDestinoId)
            .WithMessage("El origen y el destino deben ser distintos.");
    }
}
