using FluentValidation;

namespace ZuluIA_Back.Application.Features.Compras.Commands;

public class RegistrarRecepcionOrdenCompraCommandValidator : AbstractValidator<RegistrarRecepcionOrdenCompraCommand>
{
    public RegistrarRecepcionOrdenCompraCommandValidator()
    {
        RuleFor(x => x.OrdenCompraId).GreaterThan(0);
        RuleFor(x => x.CantidadRecibida).GreaterThan(0);
    }
}
