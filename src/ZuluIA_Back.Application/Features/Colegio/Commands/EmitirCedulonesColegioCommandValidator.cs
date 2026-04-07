using FluentValidation;

namespace ZuluIA_Back.Application.Features.Colegio.Commands;

public class EmitirCedulonesColegioCommandValidator : AbstractValidator<EmitirCedulonesColegioCommand>
{
    public EmitirCedulonesColegioCommandValidator()
    {
        RuleFor(x => x.LoteId).GreaterThan(0);
        RuleFor(x => x.TerceroIds).NotEmpty();
        RuleForEach(x => x.TerceroIds).GreaterThan(0);
    }
}
