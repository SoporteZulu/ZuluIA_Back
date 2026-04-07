using FluentValidation;

namespace ZuluIA_Back.Application.Features.Terceros.Commands;

public class SetTerceroUsuarioClienteParametrosBasicosCommandValidator : AbstractValidator<SetTerceroUsuarioClienteParametrosBasicosCommand>
{
    public SetTerceroUsuarioClienteParametrosBasicosCommandValidator()
    {
        RuleFor(x => x.TerceroId)
            .GreaterThan(0);

        RuleFor(x => x.DefaultSucursalId)
            .GreaterThan(0)
            .When(x => x.DefaultSucursalId.HasValue)
            .WithMessage("La sucursal por defecto indicada no es válida.");

        RuleFor(x => x.DefaultLayoutProfile)
            .MaximumLength(50)
            .When(x => !string.IsNullOrWhiteSpace(x.DefaultLayoutProfile))
            .WithMessage("El perfil de layout no puede superar los 50 caracteres.");
    }
}
