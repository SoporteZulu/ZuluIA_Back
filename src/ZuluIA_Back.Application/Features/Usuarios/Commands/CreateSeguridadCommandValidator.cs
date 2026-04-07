using FluentValidation;

namespace ZuluIA_Back.Application.Features.Usuarios.Commands;

public class CreateSeguridadCommandValidator : AbstractValidator<CreateSeguridadCommand>
{
    public CreateSeguridadCommandValidator()
    {
        RuleFor(x => x.Identificador).NotEmpty();
    }
}
