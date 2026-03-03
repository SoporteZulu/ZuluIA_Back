using FluentValidation;

namespace ZuluIA_Back.Application.Features.Usuarios.Commands;

public class UpdateUsuarioCommandValidator : AbstractValidator<UpdateUsuarioCommand>
{
    public UpdateUsuarioCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("El ID del usuario es inválido.");

        RuleFor(x => x.Email)
            .EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email))
            .WithMessage("El email no tiene un formato válido.");
    }
}