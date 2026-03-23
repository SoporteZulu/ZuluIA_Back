using FluentValidation;

namespace ZuluIA_Back.Application.Features.Referencia.Commands;

public class CreateZonaCommandValidator : AbstractValidator<CreateZonaCommand>
{
    public CreateZonaCommandValidator()
    {
        RuleFor(x => x.Descripcion)
            .NotEmpty();
    }
}

public class UpdateZonaCommandValidator : AbstractValidator<UpdateZonaCommand>
{
    public UpdateZonaCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0);

        RuleFor(x => x.Descripcion)
            .NotEmpty();
    }
}

public class ActivateZonaCommandValidator : AbstractValidator<ActivateZonaCommand>
{
    public ActivateZonaCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0);
    }
}

public class DeactivateZonaCommandValidator : AbstractValidator<DeactivateZonaCommand>
{
    public DeactivateZonaCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0);
    }
}