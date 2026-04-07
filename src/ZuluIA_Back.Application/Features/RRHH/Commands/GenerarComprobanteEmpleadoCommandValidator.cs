using FluentValidation;

namespace ZuluIA_Back.Application.Features.RRHH.Commands;

public class GenerarComprobanteEmpleadoCommandValidator : AbstractValidator<GenerarComprobanteEmpleadoCommand>
{
    public GenerarComprobanteEmpleadoCommandValidator()
    {
        RuleFor(x => x.LiquidacionSueldoId).GreaterThan(0);
        RuleFor(x => x.Tipo).NotEmpty().MaximumLength(40);
    }
}
