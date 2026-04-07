using FluentValidation;

namespace ZuluIA_Back.Application.Features.Cheques.Commands;

public class EndosarChequeCommandValidator : AbstractValidator<EndosarChequeCommand>
{
    public EndosarChequeCommandValidator()
    {
        RuleFor(x => x.ChequeId)
            .GreaterThan(0).WithMessage("El ID del cheque es obligatorio.");

        RuleFor(x => x.NuevoTerceroId)
            .GreaterThan(0).WithMessage("Debe especificar el tercero al que se endosa el cheque.");

        RuleFor(x => x.Observacion)
            .MaximumLength(500)
            .When(x => !string.IsNullOrWhiteSpace(x.Observacion))
            .WithMessage("La observación no puede exceder 500 caracteres.");
    }
}

public class AnularChequePropioCommandValidator : AbstractValidator<AnularChequePropioCommand>
{
    public AnularChequePropioCommandValidator()
    {
        RuleFor(x => x.ChequeId)
            .GreaterThan(0).WithMessage("El ID del cheque es obligatorio.");

        RuleFor(x => x.Motivo)
            .NotEmpty().WithMessage("Debe especificar el motivo de anulación.")
            .MaximumLength(500).WithMessage("El motivo no puede exceder 500 caracteres.");
    }
}

public class ActualizarChequeCommandValidator : AbstractValidator<ActualizarChequeCommand>
{
    public ActualizarChequeCommandValidator()
    {
        RuleFor(x => x.ChequeId)
            .GreaterThan(0).WithMessage("El ID del cheque es obligatorio.");

        RuleFor(x => x.Titular)
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.Titular))
            .WithMessage("El titular no puede exceder 200 caracteres.");

        RuleFor(x => x.FechaVencimiento)
            .GreaterThanOrEqualTo(x => x.FechaEmision!.Value)
            .When(x => x.FechaEmision.HasValue && x.FechaVencimiento.HasValue)
            .WithMessage("La fecha de vencimiento no puede ser anterior a la de emisión.");

        RuleFor(x => x.CodigoSucursalBancaria)
            .MaximumLength(20)
            .When(x => !string.IsNullOrWhiteSpace(x.CodigoSucursalBancaria))
            .WithMessage("El código de sucursal no puede exceder 20 caracteres.");

        RuleFor(x => x.CodigoPostal)
            .MaximumLength(20)
            .When(x => !string.IsNullOrWhiteSpace(x.CodigoPostal))
            .WithMessage("El código postal no puede exceder 20 caracteres.");
    }
}
