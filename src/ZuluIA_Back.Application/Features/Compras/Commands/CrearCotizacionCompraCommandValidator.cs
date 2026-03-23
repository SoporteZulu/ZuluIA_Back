using FluentValidation;

namespace ZuluIA_Back.Application.Features.Compras.Commands;

public class CrearCotizacionCompraCommandValidator : AbstractValidator<CrearCotizacionCompraCommand>
{
    public CrearCotizacionCompraCommandValidator()
    {
        RuleFor(x => x.SucursalId).GreaterThan(0);
        RuleFor(x => x.ProveedorId).GreaterThan(0);
        RuleFor(x => x.Fecha).NotEmpty();
        RuleFor(x => x.Items).NotEmpty();
        RuleForEach(x => x.Items).ChildRules(i =>
        {
            i.RuleFor(x => x.Descripcion).NotEmpty().MaximumLength(300);
            i.RuleFor(x => x.Cantidad).GreaterThan(0);
            i.RuleFor(x => x.PrecioUnitario).GreaterThanOrEqualTo(0);
        });
    }
}
