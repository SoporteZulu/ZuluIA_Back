using FluentValidation;

namespace ZuluIA_Back.Application.Features.Colegio.Commands;

public class FacturarCedulonesColegioCommandValidator : AbstractValidator<FacturarCedulonesColegioCommand>
{
    public FacturarCedulonesColegioCommandValidator()
    {
        RuleFor(x => x.CedulonIds).NotEmpty();
        RuleForEach(x => x.CedulonIds).GreaterThan(0);
    }
}
