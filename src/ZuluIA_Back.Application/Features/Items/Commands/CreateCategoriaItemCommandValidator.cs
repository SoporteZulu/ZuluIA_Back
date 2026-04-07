using FluentValidation;

namespace ZuluIA_Back.Application.Features.Items.Commands;

public class CreateCategoriaItemCommandValidator
    : AbstractValidator<CreateCategoriaItemCommand>
{
    public CreateCategoriaItemCommandValidator()
    {
        RuleFor(x => x.Codigo)
            .NotEmpty().WithMessage("El código es obligatorio.")
            .MaximumLength(50);

        RuleFor(x => x.Descripcion)
            .NotEmpty().WithMessage("La descripción es obligatoria.")
            .MaximumLength(200);

        RuleFor(x => x.Nivel)
            .GreaterThan((short)0).WithMessage("El nivel debe ser mayor a 0.");
    }
}

public class UpdateCategoriaItemCommandValidator
    : AbstractValidator<UpdateCategoriaItemCommand>
{
    public UpdateCategoriaItemCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0);

        RuleFor(x => x.Codigo)
            .NotEmpty().WithMessage("El código es obligatorio.")
            .MaximumLength(50);

        RuleFor(x => x.Descripcion)
            .NotEmpty().WithMessage("La descripción es obligatoria.")
            .MaximumLength(200);
    }
}

public class DeleteCategoriaItemCommandValidator
    : AbstractValidator<DeleteCategoriaItemCommand>
{
    public DeleteCategoriaItemCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0);
    }
}

public class ActivateCategoriaItemCommandValidator
    : AbstractValidator<ActivateCategoriaItemCommand>
{
    public ActivateCategoriaItemCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0);
    }
}