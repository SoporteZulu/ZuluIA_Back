using FluentValidation;

namespace ZuluIA_Back.Application.Features.Finanzas.Commands;

public class RegistrarCobroCommandValidator
    : AbstractValidator<RegistrarCobroCommand>
{
    public RegistrarCobroCommandValidator()
    {
        RuleFor(x => x.SucursalId)
            .GreaterThan(0).WithMessage("La sucursal es obligatoria.");

        RuleFor(x => x.TerceroId)
            .GreaterThan(0).WithMessage("El tercero es obligatorio.");

        RuleFor(x => x.Fecha)
            .NotEmpty().WithMessage("La fecha es obligatoria.");

        RuleFor(x => x.MonedaId)
            .GreaterThan(0).WithMessage("La moneda es obligatoria.");

        RuleFor(x => x.Cotizacion)
            .GreaterThan(0).WithMessage("La cotización debe ser mayor a 0.");

        RuleFor(x => x.Medios)
            .NotEmpty().WithMessage("Debe especificar al menos un medio de cobro.");

        RuleForEach(x => x.Medios).ChildRules(m =>
        {
            m.RuleFor(x => x.CajaId)
             .GreaterThan(0).WithMessage("La caja es obligatoria.");

            m.RuleFor(x => x.FormaPagoId)
             .GreaterThan(0).WithMessage("La forma de pago es obligatoria.");

            m.RuleFor(x => x.Importe)
             .GreaterThan(0).WithMessage("El importe debe ser mayor a 0.");
        });
    }
}