using FluentValidation;

namespace ZuluIA_Back.Application.Features.Diagnosticos.Commands;

public class CreateAspectoDiagnosticoCommandValidator : AbstractValidator<CreateAspectoDiagnosticoCommand>
{
    public CreateAspectoDiagnosticoCommandValidator()
    {
        RuleFor(x => x.RegionId).GreaterThan(0);
        RuleFor(x => x.Codigo).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Descripcion).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Peso).GreaterThan(0);
    }
}
