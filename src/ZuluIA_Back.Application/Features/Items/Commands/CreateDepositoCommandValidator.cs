using FluentValidation;

namespace ZuluIA_Back.Application.Features.Items.Commands;

public class CreateDepositoCommandValidator : AbstractValidator<CreateDepositoCommand>
{
    public CreateDepositoCommandValidator()
    {
        RuleFor(x => x.SucursalId)
            .GreaterThan(0).WithMessage("La sucursal es obligatoria.");

        RuleFor(x => x.Descripcion)
            .NotEmpty().WithMessage("La descripción es obligatoria.")
            .MaximumLength(200);
    }
}

public class UpdateDepositoCommandValidator : AbstractValidator<UpdateDepositoCommand>
{
    public UpdateDepositoCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0);

        RuleFor(x => x.Descripcion)
            .NotEmpty().WithMessage("La descripción es obligatoria.")
            .MaximumLength(200);
    }
}

public class DeleteDepositoCommandValidator : AbstractValidator<DeleteDepositoCommand>
{
    public DeleteDepositoCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0);
    }
}

public class ActivateDepositoCommandValidator : AbstractValidator<ActivateDepositoCommand>
{
    public ActivateDepositoCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0);
    }
}