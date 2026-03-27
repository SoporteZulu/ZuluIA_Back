using FluentValidation;

namespace ZuluIA_Back.Application.Features.Fiscal.Commands;

public class GenerarLibroViajantesCommandValidator : AbstractValidator<GenerarLibroViajantesCommand>
{
    public GenerarLibroViajantesCommandValidator()
    {
        RuleFor(x => x.SucursalId).GreaterThan(0);
        RuleFor(x => x.Hasta).GreaterThanOrEqualTo(x => x.Desde);
    }
}
