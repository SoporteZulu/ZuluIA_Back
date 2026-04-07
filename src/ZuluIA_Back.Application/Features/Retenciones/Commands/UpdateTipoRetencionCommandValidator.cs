using FluentValidation;

namespace ZuluIA_Back.Application.Features.Retenciones.Commands;

public class UpdateTipoRetencionCommandValidator : AbstractValidator<UpdateTipoRetencionCommand>
{
    public UpdateTipoRetencionCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("El Id es obligatorio.");
        RuleFor(x => x.Descripcion).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Regimen).NotEmpty().MaximumLength(100);
        RuleFor(x => x.MinimoNoImponible).GreaterThanOrEqualTo(0);
        RuleForEach(x => x.Escalas).ChildRules(e =>
        {
            e.RuleFor(x => x.Descripcion).NotEmpty().MaximumLength(200);
            e.RuleFor(x => x.ImporteDesde).GreaterThanOrEqualTo(0);
            e.RuleFor(x => x.Porcentaje).InclusiveBetween(0, 100);
        });
    }
}
