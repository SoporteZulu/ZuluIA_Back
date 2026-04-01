using FluentValidation;

namespace ZuluIA_Back.Application.Features.Items.Commands;

public class UpdateItemCommandValidator : AbstractValidator<UpdateItemCommand>
{
    public UpdateItemCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("El ID del ítem es inválido.");

        RuleFor(x => x.Descripcion)
            .NotEmpty().WithMessage("La descripción es obligatoria.")
            .MaximumLength(300).WithMessage("La descripción no puede superar los 300 caracteres.");

        RuleFor(x => x.DescripcionAdicional)
            .MaximumLength(500).WithMessage("La descripción adicional no puede superar los 500 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.DescripcionAdicional));

        RuleFor(x => x.CodigoBarras)
            .MaximumLength(50).WithMessage("El código de barras no puede superar los 50 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.CodigoBarras));

        RuleFor(x => x.CodigoAlternativo)
            .MaximumLength(50).WithMessage("El código alternativo no puede superar los 50 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.CodigoAlternativo));

        RuleFor(x => x.CodigoAfip)
            .MaximumLength(50).WithMessage("El código AFIP no puede superar los 50 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.CodigoAfip));

        RuleFor(x => x.UnidadMedidaId)
            .GreaterThan(0).WithMessage("Debe seleccionar una unidad de medida.");

        RuleFor(x => x.AlicuotaIvaId)
            .GreaterThan(0).WithMessage("Debe seleccionar una alícuota de IVA.");

        RuleFor(x => x.AlicuotaIvaCompraId)
            .GreaterThan(0).WithMessage("Debe seleccionar una alícuota de IVA compra válida.")
            .When(x => x.AlicuotaIvaCompraId.HasValue);

        RuleFor(x => x.MonedaId)
            .GreaterThan(0).WithMessage("Debe seleccionar una moneda.");

        RuleFor(x => x.ImpuestoInternoId)
            .GreaterThan(0).WithMessage("Debe seleccionar un impuesto interno válido.")
            .When(x => x.ImpuestoInternoId.HasValue);

        RuleFor(x => x)
            .Must(x => x.EsProducto || x.EsServicio || x.EsFinanciero)
            .WithMessage("El ítem debe ser producto, servicio o financiero.");

        RuleFor(x => x)
            .Must(x => !(x.EsProducto && x.EsServicio))
            .WithMessage("El ítem no puede ser producto y servicio a la vez.");

        RuleFor(x => x)
            .Must(x => !x.EsFinanciero || (!x.AplicaVentas.GetValueOrDefault() && !x.AplicaCompras.GetValueOrDefault()))
            .WithMessage("Un ítem financiero no puede aplicar a ventas ni compras comerciales.")
            .When(x => x.EsFinanciero && (x.AplicaVentas.HasValue || x.AplicaCompras.HasValue));

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

        RuleFor(x => x.PuntoReposicion)
            .GreaterThanOrEqualTo(0).WithMessage("El punto de reposición no puede ser negativo.")
            .When(x => x.PuntoReposicion.HasValue);

        RuleFor(x => x.StockSeguridad)
            .GreaterThanOrEqualTo(0).WithMessage("El stock de seguridad no puede ser negativo.")
            .When(x => x.StockSeguridad.HasValue);

        RuleFor(x => x.Peso)
            .GreaterThanOrEqualTo(0).WithMessage("El peso no puede ser negativo.")
            .When(x => x.Peso.HasValue);

        RuleFor(x => x.Volumen)
            .GreaterThanOrEqualTo(0).WithMessage("El volumen no puede ser negativo.")
            .When(x => x.Volumen.HasValue);

        RuleFor(x => x.DiasVencimientoLimite)
            .GreaterThanOrEqualTo(0).WithMessage("Los días límite de vencimiento no pueden ser negativos.")
            .When(x => x.DiasVencimientoLimite.HasValue);

        RuleFor(x => x)
            .Must(x => !x.EsTrazable || (x.ManejaStock && x.EsProducto && !x.EsFinanciero))
            .WithMessage("Solo un producto que maneja stock puede ser trazable.");

        RuleFor(x => x)
            .Must(x => !x.PermiteFraccionamiento || x.EsProducto)
            .WithMessage("Solo un producto puede permitir fraccionamiento.");

        RuleFor(x => x)
            .Must(x => !x.DepositoDefaultId.HasValue || (x.ManejaStock && x.EsProducto && !x.EsFinanciero))
            .WithMessage("Solo un producto que maneja stock puede tener depósito por defecto.");

        RuleFor(x => x.PorcentajeGanancia)
            .GreaterThanOrEqualTo(0).WithMessage("El porcentaje de ganancia no puede ser negativo.")
            .When(x => x.PorcentajeGanancia.HasValue);

        RuleFor(x => x.PorcentajeMaximoDescuento)
            .InclusiveBetween(0, 100).WithMessage("El porcentaje máximo de descuento debe estar entre 0 y 100.")
            .When(x => x.PorcentajeMaximoDescuento.HasValue);

        RuleForEach(x => x.AtributosComerciales).ChildRules(c =>
        {
            c.RuleFor(x => x.AtributoComercialId)
                .GreaterThan(0).WithMessage("Debe seleccionar un atributo comercial válido.");
            c.RuleFor(x => x.Valor)
                .NotEmpty().WithMessage("El valor del atributo comercial es obligatorio.")
                .MaximumLength(300).WithMessage("El valor del atributo comercial no puede superar los 300 caracteres.");
        });

        RuleFor(x => x.AtributosComerciales)
            .Must(x => x is null || x.Select(v => v.AtributoComercialId).Distinct().Count() == x.Count)
            .WithMessage("No se pueden repetir atributos comerciales en el ítem.");

        RuleForEach(x => x.Componentes).ChildRules(c =>
        {
            c.RuleFor(x => x.ComponenteId)
                .GreaterThan(0).WithMessage("Debe seleccionar un componente válido.");
            c.RuleFor(x => x.Cantidad)
                .GreaterThan(0).WithMessage("La cantidad del componente debe ser mayor a cero.");
            c.RuleFor(x => x.UnidadMedidaId)
                .GreaterThan(0).WithMessage("La unidad de medida del componente es inválida.")
                .When(x => x.UnidadMedidaId.HasValue);
        });

        RuleFor(x => x.Componentes)
            .Must(x => x is null || x.Select(v => v.ComponenteId).Distinct().Count() == x.Count)
            .WithMessage("No se pueden repetir componentes en el ítem.");

        RuleFor(x => x)
            .Must(x => x.Componentes is null || x.Componentes.All(c => c.ComponenteId != x.Id))
            .WithMessage("Un ítem no puede ser componente de sí mismo.");
    }
}