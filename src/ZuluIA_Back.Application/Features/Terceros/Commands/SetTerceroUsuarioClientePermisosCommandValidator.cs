using FluentValidation;
using System.Linq;

namespace ZuluIA_Back.Application.Features.Terceros.Commands;

public class SetTerceroUsuarioClientePermisosCommandValidator : AbstractValidator<SetTerceroUsuarioClientePermisosCommand>
{
    public SetTerceroUsuarioClientePermisosCommandValidator()
    {
        RuleFor(x => x.TerceroId)
            .GreaterThan(0);

        RuleForEach(x => x.Permisos)
            .ChildRules(item =>
            {
                item.RuleFor(x => x.SeguridadId)
                    .GreaterThan(0);
            });

        RuleFor(x => x.Permisos)
            .Must(items => items.Select(x => x.SeguridadId).Distinct().Count() == items.Count)
            .WithMessage("No puede repetir permisos básicos para el mismo usuario.");
    }
}
