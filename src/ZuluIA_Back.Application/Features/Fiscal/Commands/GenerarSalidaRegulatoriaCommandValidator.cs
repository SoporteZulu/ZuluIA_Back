using FluentValidation;

namespace ZuluIA_Back.Application.Features.Fiscal.Commands;

public class GenerarSalidaRegulatoriaCommandValidator : AbstractValidator<GenerarSalidaRegulatoriaCommand>
{
    public GenerarSalidaRegulatoriaCommandValidator()
    {
        RuleFor(x => x.SucursalId).GreaterThan(0);
        RuleFor(x => x.NombreArchivo).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Hasta).GreaterThanOrEqualTo(x => x.Desde);
    }
}
