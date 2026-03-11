using FluentValidation;

namespace ZuluIA_Back.Application.Features.Contabilidad.Commands;

public class RegistrarAsientoCommandValidator
    : AbstractValidator<RegistrarAsientoCommand>
{
    public RegistrarAsientoCommandValidator()
    {
        RuleFor(x => x.EjercicioId)
            .GreaterThan(0).WithMessage("El ejercicio es obligatorio.");

        RuleFor(x => x.SucursalId)
            .GreaterThan(0).WithMessage("La sucursal es obligatoria.");

        RuleFor(x => x.Fecha)
            .NotEmpty().WithMessage("La fecha es obligatoria.");

        RuleFor(x => x.Descripcion)
            .NotEmpty().WithMessage("La descripción es obligatoria.");

        RuleFor(x => x.Lineas)
            .NotEmpty().WithMessage("El asiento debe tener al menos una línea.");

        RuleFor(x => x.Lineas)
            .Must(lineas =>
            {
                var debe = lineas.Sum(l => l.Debe);
                var haber = lineas.Sum(l => l.Haber);
                return Math.Abs(debe - haber) <= 0.01m;
            })
            .WithMessage("El asiento no cuadra: el total de debe debe ser igual al haber.");

        RuleForEach(x => x.Lineas).ChildRules(l =>
        {
            l.RuleFor(x => x.CuentaId)
             .GreaterThan(0).WithMessage("La cuenta contable es obligatoria.");

            l.RuleFor(x => x)
             .Must(x => !(x.Debe > 0 && x.Haber > 0))
             .WithMessage("Una línea no puede tener debe y haber simultáneamente.");

            l.RuleFor(x => x)
             .Must(x => x.Debe > 0 || x.Haber > 0)
             .WithMessage("La línea debe tener un importe en debe o haber.");
        });
    }
}