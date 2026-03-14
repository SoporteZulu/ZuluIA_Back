using FluentValidation;

namespace ZuluIA_Back.Application.Features.Cajas.Commands;

public class RegistrarTransferenciaCommandValidator : AbstractValidator<RegistrarTransferenciaCommand>
{
    public RegistrarTransferenciaCommandValidator()
    {
        RuleFor(x => x.SucursalId).GreaterThan(0);
        RuleFor(x => x.CajaOrigenId).GreaterThan(0);
        RuleFor(x => x.CajaDestinoId)
            .GreaterThan(0)
            .NotEqual(x => x.CajaOrigenId)
            .WithMessage("La caja destino debe ser diferente a la caja origen.");
        RuleFor(x => x.Importe).GreaterThan(0);
        RuleFor(x => x.MonedaId).GreaterThan(0);
        RuleFor(x => x.Cotizacion).GreaterThan(0);
    }
}
