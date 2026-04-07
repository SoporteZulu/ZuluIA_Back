using FluentValidation;

namespace ZuluIA_Back.Application.Features.Colegio.Commands;

public class FacturarCedulonesColegioAutomaticoCommandValidator : AbstractValidator<FacturarCedulonesColegioAutomaticoCommand>
{
    public FacturarCedulonesColegioAutomaticoCommandValidator()
    {
        RuleFor(x => x.SucursalId).GreaterThan(0).When(x => !x.LoteId.HasValue);
    }
}
