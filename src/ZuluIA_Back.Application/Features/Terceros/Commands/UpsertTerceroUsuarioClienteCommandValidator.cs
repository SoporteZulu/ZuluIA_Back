using FluentValidation;
using System;

namespace ZuluIA_Back.Application.Features.Terceros.Commands;

public class UpsertTerceroUsuarioClienteCommandValidator : AbstractValidator<UpsertTerceroUsuarioClienteCommand>
{
    public UpsertTerceroUsuarioClienteCommandValidator()
    {
        RuleFor(x => x.TerceroId)
            .GreaterThan(0);

        RuleFor(x => x.UserName)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.UserName));

        RuleFor(x => x.UsuarioGrupoId)
            .GreaterThan(0)
            .When(x => x.UsuarioGrupoId.HasValue);

        RuleFor(x => x.Password)
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.Password));

        RuleFor(x => x)
            .Must(x => string.IsNullOrWhiteSpace(x.Password) == string.IsNullOrWhiteSpace(x.ConfirmPassword) || string.Equals(x.Password, x.ConfirmPassword, StringComparison.Ordinal))
            .WithMessage("La contraseña y su confirmación deben coincidir.");
    }
}
