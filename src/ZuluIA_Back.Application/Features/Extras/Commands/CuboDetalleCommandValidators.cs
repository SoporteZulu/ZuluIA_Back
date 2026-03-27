using FluentValidation;

namespace ZuluIA_Back.Application.Features.Extras.Commands;

public class AddCuboCampoCommandValidator : AbstractValidator<AddCuboCampoCommand>
{
    public AddCuboCampoCommandValidator()
    {
        RuleFor(x => x.CuboId).GreaterThan(0);
        RuleFor(x => x.SourceName).NotEmpty();
    }
}

public class UpdateCuboCampoCommandValidator : AbstractValidator<UpdateCuboCampoCommand>
{
    public UpdateCuboCampoCommandValidator()
    {
        RuleFor(x => x.CuboId).GreaterThan(0);
        RuleFor(x => x.CampoId).GreaterThan(0);
    }
}

public class DeleteCuboCampoCommandValidator : AbstractValidator<DeleteCuboCampoCommand>
{
    public DeleteCuboCampoCommandValidator()
    {
        RuleFor(x => x.CuboId).GreaterThan(0);
        RuleFor(x => x.CampoId).GreaterThan(0);
    }
}

public class AddCuboFiltroCommandValidator : AbstractValidator<AddCuboFiltroCommand>
{
    public AddCuboFiltroCommandValidator()
    {
        RuleFor(x => x.CuboId).GreaterThan(0);
        RuleFor(x => x.Filtro).NotEmpty();
    }
}

public class UpdateCuboFiltroCommandValidator : AbstractValidator<UpdateCuboFiltroCommand>
{
    public UpdateCuboFiltroCommandValidator()
    {
        RuleFor(x => x.CuboId).GreaterThan(0);
        RuleFor(x => x.FiltroId).GreaterThan(0);
        RuleFor(x => x.Filtro).NotEmpty();
    }
}

public class DeleteCuboFiltroCommandValidator : AbstractValidator<DeleteCuboFiltroCommand>
{
    public DeleteCuboFiltroCommandValidator()
    {
        RuleFor(x => x.CuboId).GreaterThan(0);
        RuleFor(x => x.FiltroId).GreaterThan(0);
    }
}
