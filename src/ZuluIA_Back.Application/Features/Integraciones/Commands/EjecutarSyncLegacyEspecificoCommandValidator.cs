using FluentValidation;

namespace ZuluIA_Back.Application.Features.Integraciones.Commands;

public class EjecutarSyncLegacyEspecificoCommandValidator : AbstractValidator<EjecutarSyncLegacyEspecificoCommand>
{
    public EjecutarSyncLegacyEspecificoCommandValidator()
    {
        RuleFor(x => x.Codigo).NotEmpty().MaximumLength(50);
        RuleFor(x => x.RegistrosEstimados).GreaterThanOrEqualTo(0);
    }
}
