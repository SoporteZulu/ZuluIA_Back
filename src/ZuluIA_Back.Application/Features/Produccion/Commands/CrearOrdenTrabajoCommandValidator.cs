using FluentValidation;

namespace ZuluIA_Back.Application.Features.Produccion.Commands;

public class CrearOrdenTrabajoCommandValidator
    : AbstractValidator<CrearOrdenTrabajoCommand>
{
    public CrearOrdenTrabajoCommandValidator()
    {
        RuleFor(x => x.SucursalId)
            .GreaterThan(0).WithMessage("La sucursal es obligatoria.");

        RuleFor(x => x.FormulaId)
            .GreaterThan(0).WithMessage("La fórmula es obligatoria.");

        RuleFor(x => x.DepositoOrigenId)
            .GreaterThan(0).WithMessage("El depósito de origen es obligatorio.");

        RuleFor(x => x.DepositoDestinoId)
            .GreaterThan(0).WithMessage("El depósito de destino es obligatorio.");

        RuleFor(x => x.Cantidad)
            .GreaterThan(0).WithMessage("La cantidad a producir debe ser mayor a 0.");

        RuleFor(x => x.Fecha)
            .NotEmpty().WithMessage("La fecha es obligatoria.");

        RuleFor(x => x.FechaFinPrevista)
            .GreaterThanOrEqualTo(x => x.Fecha)
            .When(x => x.FechaFinPrevista.HasValue)
            .WithMessage("La fecha de fin prevista debe ser igual o posterior a la fecha de inicio.");
    }
}