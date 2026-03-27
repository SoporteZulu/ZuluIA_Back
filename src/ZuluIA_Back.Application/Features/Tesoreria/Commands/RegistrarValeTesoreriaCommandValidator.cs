using FluentValidation;

namespace ZuluIA_Back.Application.Features.Tesoreria.Commands;

public class RegistrarValeTesoreriaCommandValidator : AbstractValidator<RegistrarValeTesoreriaCommand>
{
    public RegistrarValeTesoreriaCommandValidator()
    {
        RuleFor(x => x.SucursalId).GreaterThan(0);
        RuleFor(x => x.CajaCuentaId).GreaterThan(0);
        RuleFor(x => x.Importe).GreaterThan(0);
        RuleFor(x => x.MonedaId).GreaterThan(0);
        RuleFor(x => x.Cotizacion).GreaterThan(0);
    }
}
