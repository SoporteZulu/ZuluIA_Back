using FluentValidation;

namespace ZuluIA_Back.Application.Features.Miembros.Commands;

public class DeactivateMiembroCommandValidator : AbstractValidator<DeactivateMiembroCommand>
{
    public DeactivateMiembroCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0);
    }
}

public class ActivateMiembroCommandValidator : AbstractValidator<ActivateMiembroCommand>
{
    public ActivateMiembroCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0);
    }
}