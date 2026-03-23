using FluentValidation;

namespace ZuluIA_Back.Application.Features.Contabilidad.Commands;

public class CreateCentroCostoCommandValidator : AbstractValidator<CreateCentroCostoCommand>
{
    public CreateCentroCostoCommandValidator()
    {
        RuleFor(x => x.Codigo)
            .NotEmpty();

        RuleFor(x => x.Descripcion)
            .NotEmpty();
    }
}

public class UpdateCentroCostoCommandValidator : AbstractValidator<UpdateCentroCostoCommand>
{
    public UpdateCentroCostoCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0);

        RuleFor(x => x.Descripcion)
            .NotEmpty();
    }
}

public class DeleteCentroCostoCommandValidator : AbstractValidator<DeleteCentroCostoCommand>
{
    public DeleteCentroCostoCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0);
    }
}

public class ActivateCentroCostoCommandValidator : AbstractValidator<ActivateCentroCostoCommand>
{
    public ActivateCentroCostoCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0);
    }
}