using FluentValidation;

namespace ZuluIA_Back.Application.Features.Recibos.Commands;

public class EmitirReciboCommandValidator : AbstractValidator<EmitirReciboCommand>
{
    public EmitirReciboCommandValidator()
    {
        RuleFor(x => x.SucursalId).GreaterThan(0);
        RuleFor(x => x.TerceroId).GreaterThan(0);
        RuleFor(x => x.Fecha).NotEmpty();
        RuleFor(x => x.Serie).NotEmpty().MaximumLength(10);
        RuleFor(x => x.Items).NotEmpty();
        RuleForEach(x => x.Items).ChildRules(i =>
        {
            i.RuleFor(x => x.Descripcion).NotEmpty().MaximumLength(200);
            i.RuleFor(x => x.Importe).GreaterThan(0);
        });
    }
}
