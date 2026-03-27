using FluentValidation;

namespace ZuluIA_Back.Application.Features.Compras.Commands;

public class RecibirOrdenCompraCommandValidator : AbstractValidator<RecibirOrdenCompraCommand>
{
    public RecibirOrdenCompraCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

public class CancelarOrdenCompraCommandValidator : AbstractValidator<CancelarOrdenCompraCommand>
{
    public CancelarOrdenCompraCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
