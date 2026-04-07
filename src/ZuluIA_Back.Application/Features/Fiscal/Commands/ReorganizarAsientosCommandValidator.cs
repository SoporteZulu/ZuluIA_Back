using FluentValidation;

namespace ZuluIA_Back.Application.Features.Fiscal.Commands;

public class ReorganizarAsientosCommandValidator : AbstractValidator<ReorganizarAsientosCommand>
{
    public ReorganizarAsientosCommandValidator()
    {
        RuleFor(x => x.EjercicioId).GreaterThan(0);
        RuleFor(x => x.Hasta).GreaterThanOrEqualTo(x => x.Desde);
    }
}
