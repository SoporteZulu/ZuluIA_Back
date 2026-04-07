using FluentValidation;

namespace ZuluIA_Back.Application.Features.RRHH.Commands;

public class GenerarLiquidacionesMasivasCommandValidator : AbstractValidator<GenerarLiquidacionesMasivasCommand>
{
    public GenerarLiquidacionesMasivasCommandValidator()
    {
        RuleFor(x => x.SucursalId).GreaterThan(0);
        RuleFor(x => x.Anio).InclusiveBetween(2000, 2100);
        RuleFor(x => x.Mes).InclusiveBetween(1, 12);
        RuleFor(x => x.PorcentajeAjuste).GreaterThanOrEqualTo(-100m).When(x => x.PorcentajeAjuste.HasValue);
    }
}
