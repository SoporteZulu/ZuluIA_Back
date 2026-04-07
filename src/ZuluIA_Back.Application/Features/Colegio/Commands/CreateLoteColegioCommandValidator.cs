using FluentValidation;

namespace ZuluIA_Back.Application.Features.Colegio.Commands;

public class CreateLoteColegioCommandValidator : AbstractValidator<CreateLoteColegioCommand>
{
    public CreateLoteColegioCommandValidator()
    {
        RuleFor(x => x.PlanGeneralColegioId).GreaterThan(0);
        RuleFor(x => x.Codigo).NotEmpty().MaximumLength(50);
        RuleFor(x => x.FechaVencimiento).GreaterThanOrEqualTo(x => x.FechaEmision);
    }
}
