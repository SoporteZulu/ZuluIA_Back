using FluentValidation;

namespace ZuluIA_Back.Application.Features.RRHH.Commands;

public class ImputarLiquidacionEmpleadoCommandValidator : AbstractValidator<ImputarLiquidacionEmpleadoCommand>
{
    public ImputarLiquidacionEmpleadoCommandValidator()
    {
        RuleFor(x => x.LiquidacionSueldoId).GreaterThan(0);
        RuleFor(x => x.CajaId).GreaterThan(0);
        RuleFor(x => x.Importe).GreaterThan(0);
    }
}
