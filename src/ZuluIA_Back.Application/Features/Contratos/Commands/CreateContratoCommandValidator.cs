using FluentValidation;

namespace ZuluIA_Back.Application.Features.Contratos.Commands;

public class CreateContratoCommandValidator : AbstractValidator<CreateContratoCommand>
{
    public CreateContratoCommandValidator()
    {
        RuleFor(x => x.TerceroId).GreaterThan(0);
        RuleFor(x => x.SucursalId).GreaterThan(0);
        RuleFor(x => x.MonedaId).GreaterThan(0);
        RuleFor(x => x.Codigo).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Descripcion).NotEmpty().MaximumLength(200);
        RuleFor(x => x.FechaFin).GreaterThanOrEqualTo(x => x.FechaInicio);
        RuleFor(x => x.Importe).GreaterThanOrEqualTo(0);
    }
}
