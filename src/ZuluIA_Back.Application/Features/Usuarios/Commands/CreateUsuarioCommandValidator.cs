using FluentValidation;

namespace ZuluIA_Back.Application.Features.Usuarios.Commands;

public class CreateUsuarioCommandValidator : AbstractValidator<CreateUsuarioCommand>
{
    public CreateUsuarioCommandValidator()
    {
        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage("El nombre de usuario es obligatorio.")
            .MaximumLength(100)
            .Matches("^[a-zA-Z0-9._-]+$")
            .WithMessage("El usuario solo puede contener letras, números, puntos, guiones y guiones bajos.");

        RuleFor(x => x.Email)
            .EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email))
            .WithMessage("El email no tiene un formato válido.");

        RuleFor(x => x.Password)
            .MinimumLength(8)
            .When(x => !string.IsNullOrWhiteSpace(x.Password))
            .WithMessage("La contraseña debe tener al menos 8 caracteres.");
    }
}