using FluentValidation;

namespace ZuluIA_Back.Application.Features.Colegio.Commands;

public class UpdatePlanGeneralColegioCommandValidator : AbstractValidator<UpdatePlanGeneralColegioCommand>
{
    public UpdatePlanGeneralColegioCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.PlanPagoId).GreaterThan(0);
        RuleFor(x => x.TipoComprobanteId).GreaterThan(0);
        RuleFor(x => x.ItemId).GreaterThan(0);
        RuleFor(x => x.MonedaId).GreaterThan(0);
        RuleFor(x => x.Codigo).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Descripcion).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ImporteBase).GreaterThan(0);
    }
}
