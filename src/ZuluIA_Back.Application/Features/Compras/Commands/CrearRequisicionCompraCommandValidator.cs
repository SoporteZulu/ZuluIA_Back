using FluentValidation;

namespace ZuluIA_Back.Application.Features.Compras.Commands;

public class CrearRequisicionCompraCommandValidator : AbstractValidator<CrearRequisicionCompraCommand>
{
    public CrearRequisicionCompraCommandValidator()
    {
        RuleFor(x => x.SucursalId).GreaterThan(0);
        RuleFor(x => x.SolicitanteId).GreaterThan(0);
        RuleFor(x => x.Fecha).NotEmpty();
        RuleFor(x => x.Descripcion).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Items).NotEmpty();
        RuleForEach(x => x.Items).ChildRules(i =>
        {
            i.RuleFor(x => x.Descripcion).NotEmpty().MaximumLength(300);
            i.RuleFor(x => x.Cantidad).GreaterThan(0);
            i.RuleFor(x => x.UnidadMedida).NotEmpty().MaximumLength(20);
        });
    }
}
