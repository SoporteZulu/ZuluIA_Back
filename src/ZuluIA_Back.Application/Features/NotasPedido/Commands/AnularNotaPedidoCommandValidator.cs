using FluentValidation;

namespace ZuluIA_Back.Application.Features.NotasPedido.Commands;

public class AnularNotaPedidoCommandValidator : AbstractValidator<AnularNotaPedidoCommand>
{
    public AnularNotaPedidoCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}