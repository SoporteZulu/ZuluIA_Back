using FluentValidation;

namespace ZuluIA_Back.Application.Features.Terceros.Commands;

public class UpdateEstadoCivilCommandValidator : AbstractValidator<UpdateEstadoCivilCommand>
{
    public UpdateEstadoCivilCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Descripcion)
            .NotEmpty()
            .MaximumLength(100);
    }
}
