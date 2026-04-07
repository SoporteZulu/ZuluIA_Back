using FluentValidation;

namespace ZuluIA_Back.Application.Features.Usuarios.Commands;

public class DeleteMenuItemCommandValidator : AbstractValidator<DeleteMenuItemCommand>
{
    public DeleteMenuItemCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0);
    }
}