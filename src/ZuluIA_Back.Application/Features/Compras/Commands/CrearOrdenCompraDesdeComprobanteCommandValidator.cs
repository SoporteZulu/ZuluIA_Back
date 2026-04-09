using FluentValidation;

namespace ZuluIA_Back.Application.Features.Compras.Commands;

public class CrearOrdenCompraDesdeComprobanteCommandValidator : AbstractValidator<CrearOrdenCompraDesdeComprobanteCommand>
{
    public CrearOrdenCompraDesdeComprobanteCommandValidator()
    {
        RuleFor(x => x.ComprobanteId).GreaterThan(0);
        RuleFor(x => x.ProveedorId).GreaterThan(0);
        RuleFor(x => x.CondicionesEntrega).MaximumLength(2000);
    }
}
