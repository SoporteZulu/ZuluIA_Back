using FluentValidation;

namespace ZuluIA_Back.Application.Features.Usuarios.Commands;

public class ActivateMenuItemCommandValidator : AbstractValidator<ActivateMenuItemCommand>
{
    public ActivateMenuItemCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0);
    }
}