using FluentValidation;

namespace ZuluIA_Back.Application.Features.Contabilidad.Commands;

public class CreateAsientoCommandValidator : AbstractValidator<CreateAsientoCommand>
{
    public CreateAsientoCommandValidator()
    {
        RuleFor(x => x.EjercicioId)
            .GreaterThan(0).WithMessage("El ejercicio es obligatorio.");

        RuleFor(x => x.SucursalId)
            .GreaterThan(0).WithMessage("La sucursal es obligatoria.");

        RuleFor(x => x.Descripcion)
            .NotEmpty().WithMessage("La descripción es obligatoria.")
            .MaximumLength(500).WithMessage("La descripción no puede superar los 500 caracteres.");

        RuleFor(x => x.Lineas)
            .NotEmpty().WithMessage("El asiento debe tener al menos una línea.")
            .Must(l => l.Count >= 2).WithMessage("El asiento debe tener al menos 2 líneas.");

        RuleFor(x => x.Lineas)
            .Must(l =>
            {
                var debe = l.Sum(i => i.Debe);
                var haber = l.Sum(i => i.Haber);
                return Math.Round(debe, 2) == Math.Round(haber, 2);
            })
            .WithMessage("El asiento no balancea. El total del Debe debe ser igual al total del Haber.");

        RuleForEach(x => x.Lineas).ChildRules(l =>
        {
            l.RuleFor(i => i.CuentaId)
                .GreaterThan(0).WithMessage("La cuenta contable es obligatoria.");

            l.RuleFor(i => i)
                .Must(i => i.Debe > 0 || i.Haber > 0)
                .WithMessage("Cada línea debe tener debe o haber mayor a 0.");

            l.RuleFor(i => i)
                .Must(i => !(i.Debe > 0 && i.Haber > 0))
                .WithMessage("Una línea no puede tener debe y haber al mismo tiempo.");
        });
    }
}