using FluentValidation;

namespace ZuluIA_Back.Application.Features.Terceros.Commands;

public class CreateEstadoPersonaCommandValidator : AbstractValidator<CreateEstadoPersonaCommand>
{
    public CreateEstadoPersonaCommandValidator()
    {
        RuleFor(x => x.Descripcion)
            .NotEmpty()
            .MaximumLength(100);
    }
}
