using FluentValidation;

namespace ZuluIA_Back.Application.Features.Contratos.Commands;

public class RegistrarImpactoContratoCommandValidator : AbstractValidator<RegistrarImpactoContratoCommand>
{
    public RegistrarImpactoContratoCommandValidator()
    {
        RuleFor(x => x.ContratoId).GreaterThan(0);
        RuleFor(x => x.Importe).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Descripcion).NotEmpty().MaximumLength(300);
    }
}
