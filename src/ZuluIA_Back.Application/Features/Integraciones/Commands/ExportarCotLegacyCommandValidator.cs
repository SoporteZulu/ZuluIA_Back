using FluentValidation;

namespace ZuluIA_Back.Application.Features.Integraciones.Commands;

public class ExportarCotLegacyCommandValidator : AbstractValidator<ExportarCotLegacyCommand>
{
    public ExportarCotLegacyCommandValidator()
    {
        RuleFor(x => x.Hasta).GreaterThanOrEqualTo(x => x.Desde);
        RuleFor(x => x.SucursalId).GreaterThan(0).When(x => x.SucursalId.HasValue);
    }
}
