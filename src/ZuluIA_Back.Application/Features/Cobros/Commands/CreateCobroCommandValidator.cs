using FluentValidation;

namespace ZuluIA_Back.Application.Features.Cobros.Commands;

public class CreateCobroCommandValidator : AbstractValidator<CreateCobroCommand>
{
    public CreateCobroCommandValidator()
    {
        RuleFor(x => x.SucursalId)
            .GreaterThan(0).WithMessage("La sucursal es obligatoria.");

        RuleFor(x => x.TerceroId)
            .GreaterThan(0).WithMessage("El tercero es obligatorio.");

        RuleFor(x => x.MonedaId)
            .GreaterThan(0).WithMessage("La moneda es obligatoria.");

        RuleFor(x => x.Cotizacion)
            .GreaterThan(0).WithMessage("La cotización debe ser mayor a 0.");

        RuleFor(x => x.Medios)
            .NotEmpty().WithMessage("El cobro debe tener al menos un medio de pago.");

        RuleForEach(x => x.Medios).ChildRules(m =>
        {
            m.RuleFor(i => i.CajaId)
                .GreaterThan(0).WithMessage("La caja es obligatoria.");

            m.RuleFor(i => i.FormaPagoId)
                .GreaterThan(0).WithMessage("La forma de pago es obligatoria.");

            m.RuleFor(i => i.Importe)
                .GreaterThan(0).WithMessage("El importe debe ser mayor a 0.");
        });
    }
}