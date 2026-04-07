using FluentValidation;

namespace ZuluIA_Back.Application.Features.Ventas.Commands;

public class ActualizarCumplimientoPedidoCommandValidator : AbstractValidator<ActualizarCumplimientoPedidoCommand>
{
    public ActualizarCumplimientoPedidoCommandValidator()
    {
        RuleFor(x => x.PedidoId).GreaterThan(0);
    }
}
