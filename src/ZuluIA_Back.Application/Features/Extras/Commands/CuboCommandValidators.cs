using FluentValidation;

namespace ZuluIA_Back.Application.Features.Extras.Commands;

public class CreateCuboCommandValidator : AbstractValidator<CreateCuboCommand>
{
    public CreateCuboCommandValidator()
    {
        RuleFor(x => x.Descripcion).NotEmpty();
    }
}

public class UpdateCuboCommandValidator : AbstractValidator<UpdateCuboCommand>
{
    public UpdateCuboCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Descripcion).NotEmpty();
    }
}

public class DeleteCuboCommandValidator : AbstractValidator<DeleteCuboCommand>
{
    public DeleteCuboCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
