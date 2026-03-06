using FluentValidation;

namespace ZuluIA_Back.Application.Features.Contabilidad.Commands;

public class CreateEjercicioCommandValidator
    : AbstractValidator<CreateEjercicioCommand>
{
    public CreateEjercicioCommandValidator()
    {
        RuleFor(x => x.Descripcion)
            .NotEmpty().WithMessage("La descripción es obligatoria.");

        RuleFor(x => x.FechaInicio)
            .NotEmpty().WithMessage("La fecha de inicio es obligatoria.");

        RuleFor(x => x.FechaFin)
            .NotEmpty().WithMessage("La fecha de fin es obligatoria.")
            .GreaterThan(x => x.FechaInicio)
            .WithMessage("La fecha de fin debe ser posterior a la de inicio.");

        RuleFor(x => x.Mascara)
            .NotEmpty().WithMessage("La máscara de cuenta es obligatoria.");

        RuleFor(x => x.SucursalIds)
            .NotEmpty().WithMessage("Debe asignar al menos una sucursal.");
    }
}