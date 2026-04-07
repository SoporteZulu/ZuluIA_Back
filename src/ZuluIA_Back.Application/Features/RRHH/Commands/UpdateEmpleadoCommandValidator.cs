using FluentValidation;

namespace ZuluIA_Back.Application.Features.RRHH.Commands;

public class UpdateEmpleadoCommandValidator : AbstractValidator<UpdateEmpleadoCommand>
{
    public UpdateEmpleadoCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Cargo).NotEmpty().MaximumLength(100);
        RuleFor(x => x.SueldoBasico).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MonedaId).GreaterThan(0);
    }
}
