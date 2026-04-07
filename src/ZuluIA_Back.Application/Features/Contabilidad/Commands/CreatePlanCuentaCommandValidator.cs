using FluentValidation;

namespace ZuluIA_Back.Application.Features.Contabilidad.Commands;

public class CreatePlanCuentaCommandValidator : AbstractValidator<CreatePlanCuentaCommand>
{
    public CreatePlanCuentaCommandValidator()
    {
        RuleFor(x => x.EjercicioId).GreaterThan(0).WithMessage("El ejercicio es obligatorio.");
        RuleFor(x => x.CodigoCuenta).NotEmpty().WithMessage("El código de cuenta es obligatorio.").MaximumLength(50);
        RuleFor(x => x.Denominacion).NotEmpty().WithMessage("La denominación es obligatoria.").MaximumLength(200);
        RuleFor(x => x.Nivel).GreaterThan((short)0).WithMessage("El nivel debe ser mayor a 0.");
        RuleFor(x => x.OrdenNivel).NotEmpty().WithMessage("El orden de nivel es obligatorio.").MaximumLength(100);
    }
}
