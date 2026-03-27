using FluentValidation;

namespace ZuluIA_Back.Application.Features.OrdenesPreparacion.Commands;

public class RegistrarPickingOrdenPreparacionCommandValidator : AbstractValidator<RegistrarPickingOrdenPreparacionCommand>
{
    public RegistrarPickingOrdenPreparacionCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Detalles).NotEmpty();
        RuleForEach(x => x.Detalles).ChildRules(c =>
        {
            c.RuleFor(x => x.DetalleId).GreaterThan(0);
            c.RuleFor(x => x.CantidadEntregada).GreaterThanOrEqualTo(0);
        });
    }
}
