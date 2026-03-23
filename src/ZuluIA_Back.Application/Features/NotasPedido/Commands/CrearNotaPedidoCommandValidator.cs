using FluentValidation;

namespace ZuluIA_Back.Application.Features.NotasPedido.Commands;

public class CrearNotaPedidoCommandValidator : AbstractValidator<CrearNotaPedidoCommand>
{
    public CrearNotaPedidoCommandValidator()
    {
        RuleFor(x => x.SucursalId).GreaterThan(0);
        RuleFor(x => x.TerceroId).GreaterThan(0);
        RuleFor(x => x.Fecha).NotEmpty();
        RuleFor(x => x.Items).NotEmpty();
        RuleForEach(x => x.Items).ChildRules(i =>
        {
            i.RuleFor(x => x.ItemId).GreaterThan(0);
            i.RuleFor(x => x.Cantidad).GreaterThan(0);
            i.RuleFor(x => x.PrecioUnitario).GreaterThanOrEqualTo(0);
            i.RuleFor(x => x.Bonificacion).InclusiveBetween(0, 100);
        });
    }
}
