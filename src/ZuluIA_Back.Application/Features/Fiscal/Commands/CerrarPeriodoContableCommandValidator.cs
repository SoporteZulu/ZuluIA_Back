using FluentValidation;

namespace ZuluIA_Back.Application.Features.Fiscal.Commands;

public class CerrarPeriodoContableCommandValidator : AbstractValidator<CerrarPeriodoContableCommand>
{
    public CerrarPeriodoContableCommandValidator()
    {
        RuleFor(x => x.EjercicioId).GreaterThan(0);
        RuleFor(x => x.Hasta).GreaterThanOrEqualTo(x => x.Desde);
    }
}
