using FluentValidation;

namespace ZuluIA_Back.Application.Features.Usuarios.Commands;

public class CreateMenuItemCommandValidator : AbstractValidator<CreateMenuItemCommand>
{
    public CreateMenuItemCommandValidator()
    {
        RuleFor(x => x.Descripcion)
            .NotEmpty()
            .MaximumLength(200);
    }
}