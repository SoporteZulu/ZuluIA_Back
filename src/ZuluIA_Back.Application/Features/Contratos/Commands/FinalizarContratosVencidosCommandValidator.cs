using FluentValidation;

namespace ZuluIA_Back.Application.Features.Contratos.Commands;

public class FinalizarContratosVencidosCommandValidator : AbstractValidator<FinalizarContratosVencidosCommand>
{
    public FinalizarContratosVencidosCommandValidator()
    {
        RuleFor(x => x.SucursalId).GreaterThan(0).When(x => x.SucursalId.HasValue);
    }
}
