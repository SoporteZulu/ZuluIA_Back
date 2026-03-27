using FluentValidation;

namespace ZuluIA_Back.Application.Features.Usuarios.Commands;

public class UpdateMenuItemCommandValidator : AbstractValidator<UpdateMenuItemCommand>
{
    public UpdateMenuItemCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0);

        RuleFor(x => x.Descripcion)
            .NotEmpty()
            .MaximumLength(200);
    }
}