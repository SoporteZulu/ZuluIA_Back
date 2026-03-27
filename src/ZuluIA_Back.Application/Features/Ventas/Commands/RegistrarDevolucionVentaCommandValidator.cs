using FluentValidation;

namespace ZuluIA_Back.Application.Features.Ventas.Commands;

public class RegistrarDevolucionVentaCommandValidator : AbstractValidator<RegistrarDevolucionVentaCommand>
{
    public RegistrarDevolucionVentaCommandValidator()
    {
        RuleFor(x => x.SucursalId).GreaterThan(0);
        RuleFor(x => x.TipoComprobanteId).GreaterThan(0);
        RuleFor(x => x.TerceroId).GreaterThan(0);
        RuleFor(x => x.MonedaId).GreaterThan(0);
        RuleFor(x => x.Cotizacion).GreaterThan(0);
        RuleFor(x => x.Items).NotEmpty();
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ItemId).GreaterThan(0);
            item.RuleFor(i => i.Cantidad).GreaterThan(0);
            item.RuleFor(i => i.AlicuotaIvaId).GreaterThan(0);
        });
    }
}
