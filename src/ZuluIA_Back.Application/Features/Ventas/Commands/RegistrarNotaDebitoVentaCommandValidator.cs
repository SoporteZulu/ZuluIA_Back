using FluentValidation;

namespace ZuluIA_Back.Application.Features.Ventas.Commands;

public class RegistrarNotaDebitoVentaCommandValidator : AbstractValidator<RegistrarNotaDebitoVentaCommand>
{
    public RegistrarNotaDebitoVentaCommandValidator()
    {
        RuleFor(x => x.SucursalId).GreaterThan(0).WithMessage("La sucursal es obligatoria.");
        RuleFor(x => x.TipoComprobanteId).GreaterThan(0).WithMessage("El tipo de comprobante es obligatorio.");
        RuleFor(x => x.TerceroId).GreaterThan(0).WithMessage("El cliente es obligatorio.");
        RuleFor(x => x.MonedaId).GreaterThan(0).WithMessage("La moneda es obligatoria.");
        RuleFor(x => x.Cotizacion).GreaterThan(0).WithMessage("La cotización debe ser mayor a 0.");
        RuleFor(x => x.Percepciones).GreaterThanOrEqualTo(0).WithMessage("Las percepciones no pueden ser negativas.");
        RuleFor(x => x.MotivoDebitoId).GreaterThan(0).WithMessage("El motivo de débito es obligatorio.");
        RuleFor(x => x.Items).NotEmpty().WithMessage("La nota de débito debe tener al menos un ítem.");
        RuleFor(x => x.MotivoDebitoObservacion).MaximumLength(1000);

        RuleFor(x => x.ListaPreciosId)
            .GreaterThan(0)
            .When(x => x.ListaPreciosId.HasValue)
            .WithMessage("La lista de precios es inválida.");

        RuleFor(x => x.VendedorId)
            .GreaterThan(0)
            .When(x => x.VendedorId.HasValue)
            .WithMessage("El vendedor es inválido.");

        RuleFor(x => x.CanalVentaId)
            .GreaterThan(0)
            .When(x => x.CanalVentaId.HasValue)
            .WithMessage("El canal de venta es inválido.");

        RuleFor(x => x.CondicionPagoId)
            .GreaterThan(0)
            .When(x => x.CondicionPagoId.HasValue)
            .WithMessage("La condición de pago es inválida.");

        RuleFor(x => x.PlazoDias)
            .GreaterThanOrEqualTo(0)
            .When(x => x.PlazoDias.HasValue)
            .WithMessage("El plazo de días no puede ser negativo.");

        RuleFor(x => x.ComprobanteOrigenId)
            .GreaterThan(0)
            .When(x => x.ComprobanteOrigenId.HasValue)
            .WithMessage("El comprobante origen es inválido.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ItemId).GreaterThan(0).WithMessage("El ID del ítem es inválido.");
            item.RuleFor(i => i.Cantidad).GreaterThan(0).WithMessage("La cantidad debe ser mayor a 0.");
            item.RuleFor(i => i.AlicuotaIvaId).GreaterThan(0).WithMessage("La alícuota de IVA es obligatoria.");
            item.RuleFor(i => i.PrecioUnitario).GreaterThanOrEqualTo(0).WithMessage("El precio unitario no puede ser negativo.");
            item.RuleFor(i => i.DescuentoPct).InclusiveBetween(0m, 100m).WithMessage("El descuento debe estar entre 0 y 100.");

            item.RuleFor(i => i.ComprobanteItemOrigenId)
                .GreaterThan(0)
                .When(i => i.ComprobanteItemOrigenId.HasValue)
                .WithMessage("El renglón origen es inválido.");

            item.RuleFor(i => i.CantidadDocumentoOrigen)
                .GreaterThan(0)
                .When(i => i.CantidadDocumentoOrigen.HasValue)
                .WithMessage("La cantidad del documento origen debe ser mayor a 0.");

            item.RuleFor(i => i.PrecioDocumentoOrigen)
                .GreaterThanOrEqualTo(0)
                .When(i => i.PrecioDocumentoOrigen.HasValue)
                .WithMessage("El precio del documento origen no puede ser negativo.");
        });
    }
}
