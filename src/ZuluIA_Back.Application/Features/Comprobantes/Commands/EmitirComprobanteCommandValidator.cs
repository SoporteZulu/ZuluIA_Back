using FluentValidation;

namespace ZuluIA_Back.Application.Features.Comprobantes.Commands;

public class EmitirComprobanteCommandValidator
    : AbstractValidator<EmitirComprobanteCommand>
{
    public EmitirComprobanteCommandValidator()
    {
        RuleFor(x => x.SucursalId)
            .GreaterThan(0).WithMessage("La sucursal es obligatoria.");

        RuleFor(x => x.TipoComprobanteId)
            .GreaterThan(0).WithMessage("El tipo de comprobante es obligatorio.");

        RuleFor(x => x.TerceroId)
            .GreaterThan(0).WithMessage("El tercero es obligatorio.");

        RuleFor(x => x.MonedaId)
            .GreaterThan(0).WithMessage("La moneda es obligatoria.");

        RuleFor(x => x.Cotizacion)
            .GreaterThan(0).WithMessage("La cotización debe ser mayor a 0.");

        RuleFor(x => x.Fecha)
            .NotEmpty().WithMessage("La fecha es obligatoria.");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("El comprobante debe tener al menos un ítem.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.ItemId)
                .GreaterThan(0).WithMessage("El ítem es obligatorio.");

            item.RuleFor(x => x.Cantidad)
                .GreaterThan(0).WithMessage("La cantidad debe ser mayor a 0.");

            item.RuleFor(x => x.PrecioUnitario)
                .GreaterThanOrEqualTo(0)
                .WithMessage("El precio unitario no puede ser negativo.");

            item.RuleFor(x => x.DescuentoPct)
                .InclusiveBetween(0, 100)
                .WithMessage("El descuento debe estar entre 0 y 100.");
        });
    }
}