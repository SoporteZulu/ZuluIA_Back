using FluentValidation;

namespace ZuluIA_Back.Application.Features.Terceros.Commands;

public class UpdateTerceroCommandValidator : AbstractValidator<UpdateTerceroCommand>
{
    public UpdateTerceroCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("El ID del tercero es inválido.");

        RuleFor(x => x.RazonSocial)
            .NotEmpty().WithMessage("La razón social es obligatoria.")
            .MaximumLength(200).WithMessage("La razón social no puede superar los 200 caracteres.");

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("El email no tiene un formato válido.")
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x.LimiteCredito)
            .GreaterThanOrEqualTo(0).WithMessage("El límite de crédito no puede ser negativo.")
            .When(x => x.LimiteCredito.HasValue);
    }
}