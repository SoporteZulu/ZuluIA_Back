using FluentValidation;

namespace ZuluIA_Back.Application.Features.Contratos.Commands;

public class UpdateContratoCommandValidator : AbstractValidator<UpdateContratoCommand>
{
    public UpdateContratoCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Descripcion).NotEmpty().MaximumLength(200);
        RuleFor(x => x.FechaFin).GreaterThanOrEqualTo(x => x.FechaInicio);
        RuleFor(x => x.Importe).GreaterThanOrEqualTo(0);
    }
}
