using FluentValidation;

namespace ZuluIA_Back.Application.Features.PlanesPago.Commands;

public class CreatePlanPagoCommandValidator : AbstractValidator<CreatePlanPagoCommand>
{
    public CreatePlanPagoCommandValidator()
    {
        RuleFor(x => x.Descripcion)
            .NotEmpty().WithMessage("La descripción es obligatoria.")
            .MaximumLength(200);

        RuleFor(x => x.CantidadCuotas)
            .GreaterThanOrEqualTo((short)1)
            .WithMessage("La cantidad de cuotas debe ser al menos 1.");

        RuleFor(x => x.InteresPct)
            .GreaterThanOrEqualTo(0)
            .WithMessage("El interés no puede ser negativo.");
    }
}