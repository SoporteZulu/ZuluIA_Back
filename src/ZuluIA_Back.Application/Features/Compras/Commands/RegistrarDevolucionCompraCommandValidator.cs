using FluentValidation;

namespace ZuluIA_Back.Application.Features.Compras.Commands;

public class RegistrarDevolucionCompraCommandValidator : AbstractValidator<RegistrarDevolucionCompraCommand>
{
    public RegistrarDevolucionCompraCommandValidator()
    {
        RuleFor(x => x.SucursalId).GreaterThan(0);
        RuleFor(x => x.TipoComprobanteId).GreaterThan(0);
        RuleFor(x => x.TerceroId).GreaterThan(0);
        RuleFor(x => x.MonedaId).GreaterThan(0);
        RuleFor(x => x.Cotizacion).GreaterThan(0);
        RuleFor(x => x.Items).NotEmpty();
    }
}
