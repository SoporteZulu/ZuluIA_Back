using FluentValidation;

namespace ZuluIA_Back.Application.Features.Produccion.Commands;

public class FinalizarOrdenTrabajoCommandValidator : AbstractValidator<FinalizarOrdenTrabajoCommand>
{
    public FinalizarOrdenTrabajoCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);

        RuleFor(x => x.CantidadProducida)
            .GreaterThan(0)
            .When(x => x.CantidadProducida.HasValue);

        RuleForEach(x => x.Consumos).ChildRules(consumo =>
        {
            consumo.RuleFor(x => x.ItemId).GreaterThan(0);
            consumo.RuleFor(x => x.CantidadConsumida).GreaterThanOrEqualTo(0);
        });
    }
}
