using FluentValidation;

namespace ZuluIA_Back.Application.Features.Produccion.Commands;

public class CrearOrdenEmpaqueCommandValidator : AbstractValidator<CrearOrdenEmpaqueCommand>
{
    public CrearOrdenEmpaqueCommandValidator()
    {
        RuleFor(x => x.OrdenTrabajoId).GreaterThan(0);
        RuleFor(x => x.ItemId).GreaterThan(0);
        RuleFor(x => x.DepositoId).GreaterThan(0);
        RuleFor(x => x.Cantidad).GreaterThan(0);
    }
}
