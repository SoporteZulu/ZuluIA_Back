using FluentValidation;

namespace ZuluIA_Back.Application.Features.Fiscal.Commands;

public class RegistrarCierrePeriodoContableCommandValidator : AbstractValidator<RegistrarCierrePeriodoContableCommand>
{
    public RegistrarCierrePeriodoContableCommandValidator()
    {
        RuleFor(x => x.EjercicioId).GreaterThan(0);
        RuleFor(x => x.Hasta).GreaterThanOrEqualTo(x => x.Desde);
    }
}
