using FluentValidation;

namespace ZuluIA_Back.Application.Features.Integraciones.Commands;

public class EjecutarSyncIntegracionCommandValidator : AbstractValidator<EjecutarSyncIntegracionCommand>
{
    public EjecutarSyncIntegracionCommandValidator()
    {
        RuleFor(x => x.Tipo).IsInEnum();
        RuleFor(x => x.CodigoMonitor).NotEmpty().MaximumLength(50);
        RuleFor(x => x.DescripcionMonitor).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ClaveIdempotencia).MaximumLength(100).When(x => !string.IsNullOrWhiteSpace(x.ClaveIdempotencia));
    }
}
