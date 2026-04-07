using FluentValidation;

namespace ZuluIA_Back.Application.Features.Terceros.Commands;

public class CreateEstadoCivilCommandValidator : AbstractValidator<CreateEstadoCivilCommand>
{
    public CreateEstadoCivilCommandValidator()
    {
        RuleFor(x => x.Descripcion)
            .NotEmpty()
            .MaximumLength(100);
    }
}
