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