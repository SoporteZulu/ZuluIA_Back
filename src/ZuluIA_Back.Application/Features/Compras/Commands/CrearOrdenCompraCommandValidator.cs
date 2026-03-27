using FluentValidation;

namespace ZuluIA_Back.Application.Features.Compras.Commands;

public class CrearOrdenCompraCommandValidator : AbstractValidator<CrearOrdenCompraCommand>
{
    public CrearOrdenCompraCommandValidator()
    {
        RuleFor(x => x.SucursalId).GreaterThan(0);
        RuleFor(x => x.TipoComprobanteId).GreaterThan(0);
        RuleFor(x => x.TerceroId).GreaterThan(0);
        RuleFor(x => x.MonedaId).GreaterThan(0);
        RuleFor(x => x.Cotizacion).GreaterThan(0);
        RuleFor(x => x.Items).NotEmpty();
    }
}
