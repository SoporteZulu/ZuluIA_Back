using FluentValidation;

namespace ZuluIA_Back.Application.Features.Terceros.Commands;

public class UpdateEstadoPersonaCommandValidator : AbstractValidator<UpdateEstadoPersonaCommand>
{
    public UpdateEstadoPersonaCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Descripcion)
            .NotEmpty()
            .MaximumLength(100);
    }
}
