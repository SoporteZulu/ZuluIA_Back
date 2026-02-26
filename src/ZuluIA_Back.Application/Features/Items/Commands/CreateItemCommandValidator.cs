using FluentValidation;

namespace ZuluIA_Back.Application.Features.Items.Commands;

public class CreateItemCommandValidator : AbstractValidator<CreateItemCommand>
{
    public CreateItemCommandValidator()
    {
        RuleFor(x => x.Codigo)
            .NotEmpty().WithMessage("El código es obligatorio.")
            .MaximumLength(30).WithMessage("El código no puede superar los 30 caracteres.");

        RuleFor(x => x.Descripcion)
            .NotEmpty().WithMessage("La descripción es obligatoria.")
            .MaximumLength(200).WithMessage("La descripción no puede superar los 200 caracteres.");

        RuleFor(x => x.UnidadMedidaId)
            .GreaterThan(0).WithMessage("Debe seleccionar una unidad de medida.");

        RuleFor(x => x.AlicuotaIvaId)
            .GreaterThan(0).WithMessage("Debe seleccionar una alícuota de IVA.");

        RuleFor(x => x.MonedaId)
            .GreaterThan(0).WithMessage("Debe seleccionar una moneda.");

        RuleFor(x => x)
            .Must(x => x.EsProducto || x.EsServicio)
            .WithMessage("El ítem debe ser producto o servicio.");

        RuleFor(x => x)
            .Must(x => !(x.EsProducto && x.EsServicio))
            .WithMessage("El ítem no puede ser producto y servicio a la vez.");

        RuleFor(x => x.PrecioCosto)
            .GreaterThanOrEqualTo(0).WithMessage("El precio de costo no puede ser negativo.");

        RuleFor(x => x.PrecioVenta)
            .GreaterThanOrEqualTo(0).WithMessage("El precio de venta no puede ser negativo.");

        RuleFor(x => x.StockMinimo)
            .GreaterThanOrEqualTo(0).WithMessage("El stock mínimo no puede ser negativo.");

        RuleFor(x => x.StockMaximo)
            .GreaterThanOrEqualTo(x => x.StockMinimo)
            .WithMessage("El stock máximo no puede ser menor al stock mínimo.")
            .When(x => x.StockMaximo.HasValue);
    }
}