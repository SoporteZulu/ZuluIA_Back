using FluentValidation;

namespace ZuluIA_Back.Application.Features.Fiscal.Commands;

public class RegistrarLiquidacionPrimariaGranoCommandValidator : AbstractValidator<RegistrarLiquidacionPrimariaGranoCommand>
{
    public RegistrarLiquidacionPrimariaGranoCommandValidator()
    {
        RuleFor(x => x.SucursalId).GreaterThan(0);
        RuleFor(x => x.NumeroLiquidacion).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Producto).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Cantidad).GreaterThan(0);
        RuleFor(x => x.PrecioUnitario).GreaterThan(0);
    }
}
