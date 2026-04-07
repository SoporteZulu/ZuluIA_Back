using FluentValidation;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Application.Features.Cheques.Commands;

public class CreateChequeCommandValidator : AbstractValidator<CreateChequeCommand>
{
    public CreateChequeCommandValidator()
    {
        RuleFor(x => x.CajaId)
            .GreaterThan(0).WithMessage("La caja es obligatoria.");
        
        RuleFor(x => x.NroCheque)
            .NotEmpty().WithMessage("El número de cheque es obligatorio.")
            .MaximumLength(50).WithMessage("El número de cheque no puede exceder 50 caracteres.");
        
        RuleFor(x => x.Banco)
            .NotEmpty().WithMessage("El banco es obligatorio.")
            .MaximumLength(100).WithMessage("El banco no puede exceder 100 caracteres.");
        
        RuleFor(x => x.Importe)
            .GreaterThan(0).WithMessage("El importe debe ser mayor a 0.");
        
        RuleFor(x => x.MonedaId)
            .GreaterThan(0).WithMessage("La moneda es obligatoria.");

        RuleFor(x => x.Tipo)
            .IsInEnum();
        
        RuleFor(x => x.FechaVencimiento)
            .GreaterThanOrEqualTo(x => x.FechaEmision)
            .WithMessage("La fecha de vencimiento no puede ser anterior a la de emisión.");
        
        RuleFor(x => x.Titular)
            .NotEmpty()
            .When(x => x.Tipo == TipoCheque.Tercero)
            .WithMessage("El titular es obligatorio para cheques de terceros.");
        RuleFor(x => x.Titular)
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.Titular))
            .WithMessage("El titular no puede exceder 200 caracteres.");
        
        RuleFor(x => x.ChequeraId)
            .NotNull()
            .When(x => x.Tipo == TipoCheque.Propio)
            .WithMessage("La chequera es obligatoria para cheques propios.");
        RuleFor(x => x.ChequeraId)
            .GreaterThan(0)
            .When(x => x.Tipo == TipoCheque.Propio && x.ChequeraId.HasValue)
            .WithMessage("La chequera debe ser válida.");
        
        RuleFor(x => x.CodigoSucursalBancaria)
            .MaximumLength(20)
            .When(x => !string.IsNullOrWhiteSpace(x.CodigoSucursalBancaria))
            .WithMessage("El código de sucursal no puede exceder 20 caracteres.");
        
        RuleFor(x => x.Cuit)
            .MaximumLength(20)
            .When(x => !string.IsNullOrWhiteSpace(x.Cuit))
            .WithMessage("El CUIT no puede exceder 20 caracteres.");

        RuleFor(x => x.Plaza)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.Plaza))
            .WithMessage("La plaza no puede exceder 100 caracteres.");
        
        RuleFor(x => x.Observacion)
            .MaximumLength(500)
            .When(x => !string.IsNullOrWhiteSpace(x.Observacion))
            .WithMessage("La observación no puede exceder 500 caracteres.");
    }
}