using FluentValidation;

namespace ZuluIA_Back.Application.Features.Tesoreria.Commands;

public class RegistrarDepositoOperarCommandValidator : AbstractValidator<RegistrarDepositoOperarCommand>
{
    public RegistrarDepositoOperarCommandValidator()
    {
        RuleFor(x => x.SucursalId).GreaterThan(0);
        RuleFor(x => x.CajaOrigenId).GreaterThan(0);
        RuleFor(x => x.CajaDestinoId).GreaterThan(0).NotEqual(x => x.CajaOrigenId);
        RuleFor(x => x.Importe).GreaterThan(0);
        RuleFor(x => x.MonedaId).GreaterThan(0);
        RuleFor(x => x.Cotizacion).GreaterThan(0);
    }
}
