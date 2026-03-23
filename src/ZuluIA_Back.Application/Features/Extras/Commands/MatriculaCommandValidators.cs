using FluentValidation;

namespace ZuluIA_Back.Application.Features.Extras.Commands;

public class CreateMatriculaCommandValidator : AbstractValidator<CreateMatriculaCommand>
{
    public CreateMatriculaCommandValidator()
    {
        RuleFor(x => x.TerceroId).GreaterThan(0);
        RuleFor(x => x.SucursalId).GreaterThan(0);
        RuleFor(x => x.NroMatricula).NotEmpty();
    }
}

public class UpdateMatriculaCommandValidator : AbstractValidator<UpdateMatriculaCommand>
{
    public UpdateMatriculaCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

public class ActivateMatriculaCommandValidator : AbstractValidator<ActivateMatriculaCommand>
{
    public ActivateMatriculaCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

public class DeactivateMatriculaCommandValidator : AbstractValidator<DeactivateMatriculaCommand>
{
    public DeactivateMatriculaCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
