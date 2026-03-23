using FluentValidation;

namespace ZuluIA_Back.Application.Features.Extras.Commands;

public class CreateRegionCommandValidator : AbstractValidator<CreateRegionCommand>
{
    public CreateRegionCommandValidator()
    {
        RuleFor(x => x.Codigo).NotEmpty();
        RuleFor(x => x.Descripcion).NotEmpty();
    }
}

public class UpdateRegionCommandValidator : AbstractValidator<UpdateRegionCommand>
{
    public UpdateRegionCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Descripcion).NotEmpty();
    }
}

public class DeleteRegionCommandValidator : AbstractValidator<DeleteRegionCommand>
{
    public DeleteRegionCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
