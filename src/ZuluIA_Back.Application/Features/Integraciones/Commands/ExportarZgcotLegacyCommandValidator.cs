using FluentValidation;

namespace ZuluIA_Back.Application.Features.Integraciones.Commands;

public class ExportarZgcotLegacyCommandValidator : AbstractValidator<ExportarZgcotLegacyCommand>
{
    public ExportarZgcotLegacyCommandValidator()
    {
        RuleFor(x => x.Hasta).GreaterThanOrEqualTo(x => x.Desde);
        RuleFor(x => x.SucursalId).GreaterThan(0).When(x => x.SucursalId.HasValue);
    }
}
