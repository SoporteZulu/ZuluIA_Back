using FluentValidation;

namespace ZuluIA_Back.Application.Features.Fiscal.Commands;

public class GenerarRentasBsAsCommandValidator : AbstractValidator<GenerarRentasBsAsCommand>
{
    public GenerarRentasBsAsCommandValidator()
    {
        RuleFor(x => x.SucursalId).GreaterThan(0);
        RuleFor(x => x.Hasta).GreaterThanOrEqualTo(x => x.Desde);
    }
}
