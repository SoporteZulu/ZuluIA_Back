using FluentValidation;

namespace ZuluIA_Back.Application.Features.Ventas.Commands;

public class CrearBorradorVentaCommandValidator : AbstractValidator<CrearBorradorVentaCommand>
{
    public CrearBorradorVentaCommandValidator()
    {
        RuleFor(x => x.SucursalId).GreaterThan(0);
        RuleFor(x => x.TipoComprobanteId).GreaterThan(0);
        RuleFor(x => x.TerceroId).GreaterThan(0);
        RuleFor(x => x.MonedaId).GreaterThan(0);
        RuleFor(x => x.Cotizacion).GreaterThan(0);

        RuleFor(x => x.ListaPreciosId).GreaterThan(0).When(x => x.ListaPreciosId.HasValue);
        RuleFor(x => x.VendedorId).GreaterThan(0).When(x => x.VendedorId.HasValue);
        RuleFor(x => x.CanalVentaId).GreaterThan(0).When(x => x.CanalVentaId.HasValue);
        RuleFor(x => x.CondicionPagoId).GreaterThan(0).When(x => x.CondicionPagoId.HasValue);
        RuleFor(x => x.PlazoDias).GreaterThanOrEqualTo(0).When(x => x.PlazoDias.HasValue);

        RuleFor(x => x.Items)
            .NotEmpty()
            .WithMessage("El documento debe contener al menos un ítem.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ItemId).GreaterThan(0);
            item.RuleFor(i => i.Cantidad).GreaterThan(0);
            item.RuleFor(i => i.PrecioUnitario).GreaterThanOrEqualTo(0);
            item.RuleFor(i => i.AlicuotaIvaId).GreaterThan(0);
        });
    }
}
