using FluentValidation;

namespace ZuluIA_Back.Application.Features.Diagnosticos.Commands;

public class CreatePlanillaDiagnosticaCommandValidator : AbstractValidator<CreatePlanillaDiagnosticaCommand>
{
    public CreatePlanillaDiagnosticaCommandValidator()
    {
        RuleFor(x => x.PlantillaId).GreaterThan(0);
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(300);
    }
}
