using FluentValidation;

namespace ZuluIA_Back.Application.Features.Ventas.Commands;

public class CerrarPedidoCommandValidator : AbstractValidator<CerrarPedidoCommand>
{
    public CerrarPedidoCommandValidator()
    {
        RuleFor(x => x.PedidoId).GreaterThan(0);
        RuleFor(x => x.MotivoCierre).MaximumLength(500);
    }
}
