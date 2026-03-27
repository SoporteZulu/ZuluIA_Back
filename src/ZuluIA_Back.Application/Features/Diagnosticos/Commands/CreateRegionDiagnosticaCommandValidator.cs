using FluentValidation;

namespace ZuluIA_Back.Application.Features.Diagnosticos.Commands;

public class CreateRegionDiagnosticaCommandValidator : AbstractValidator<CreateRegionDiagnosticaCommand>
{
    public CreateRegionDiagnosticaCommandValidator()
    {
        RuleFor(x => x.Codigo).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Descripcion).NotEmpty().MaximumLength(200);
    }
}
