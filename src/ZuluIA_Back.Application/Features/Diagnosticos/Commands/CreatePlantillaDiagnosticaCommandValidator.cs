using FluentValidation;

namespace ZuluIA_Back.Application.Features.Diagnosticos.Commands;

public class CreatePlantillaDiagnosticaCommandValidator : AbstractValidator<CreatePlantillaDiagnosticaCommand>
{
    public CreatePlantillaDiagnosticaCommandValidator()
    {
        RuleFor(x => x.Codigo).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Descripcion).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Variables).NotEmpty();
    }
}
