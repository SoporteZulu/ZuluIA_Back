using FluentValidation;

namespace ZuluIA_Back.Application.Features.Items.Commands;

public class SetAtributoItemCommandValidator : AbstractValidator<SetAtributoItemCommand>
{
    public SetAtributoItemCommandValidator()
    {
        RuleFor(x => x.ItemId)
            .GreaterThan(0);

        RuleFor(x => x.AtributoId)
            .GreaterThan(0);
    }
}

public class DeleteAtributoItemCommandValidator : AbstractValidator<DeleteAtributoItemCommand>
{
    public DeleteAtributoItemCommandValidator()
    {
        RuleFor(x => x.ItemId)
            .GreaterThan(0);

        RuleFor(x => x.AtributoId)
            .GreaterThan(0);
    }
}