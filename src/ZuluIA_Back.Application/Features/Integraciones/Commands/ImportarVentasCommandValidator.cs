using FluentValidation;

namespace ZuluIA_Back.Application.Features.Integraciones.Commands;

public class ImportarVentasCommandValidator : AbstractValidator<ImportarVentasCommand>
{
    public ImportarVentasCommandValidator()
    {
        RuleFor(x => x.Ventas).NotEmpty();
        RuleForEach(x => x.Ventas).ChildRules(venta =>
        {
            venta.RuleFor(x => x.ReferenciaExterna).NotEmpty();
            venta.RuleFor(x => x.SucursalId).GreaterThan(0);
            venta.RuleFor(x => x.TipoComprobanteId).GreaterThan(0);
            venta.RuleFor(x => x.TerceroId).GreaterThan(0);
            venta.RuleFor(x => x.MonedaId).GreaterThan(0);
            venta.RuleFor(x => x.Cotizacion).GreaterThan(0);
            venta.RuleFor(x => x.Items).NotEmpty();
        });
    }
}
