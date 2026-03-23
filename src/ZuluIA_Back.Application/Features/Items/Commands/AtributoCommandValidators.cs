using FluentValidation;

namespace ZuluIA_Back.Application.Features.Items.Commands;

public class CreateAtributoCommandValidator : AbstractValidator<CreateAtributoCommand>
{
    public CreateAtributoCommandValidator()
    {
        RuleFor(x => x.Descripcion).NotEmpty();
        RuleFor(x => x.Tipo).NotEmpty();
    }
}

public class UpdateAtributoCommandValidator : AbstractValidator<UpdateAtributoCommand>
{
    public UpdateAtributoCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Descripcion).NotEmpty();
        RuleFor(x => x.Tipo).NotEmpty();
    }
}

public class ActivateAtributoCommandValidator : AbstractValidator<ActivateAtributoCommand>
{
    public ActivateAtributoCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

public class DeactivateAtributoCommandValidator : AbstractValidator<DeactivateAtributoCommand>
{
    public DeactivateAtributoCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}