using FluentValidation;

namespace ZuluIA_Back.Application.Features.Cheques.Commands;

public class CreateChequeCommandValidator : AbstractValidator<CreateChequeCommand>
{
    public CreateChequeCommandValidator()
    {
        RuleFor(x => x.CajaId).GreaterThan(0).WithMessage("La caja es obligatoria.");
        RuleFor(x => x.NroCheque).NotEmpty().WithMessage("El número de cheque es obligatorio.").MaximumLength(50);
        RuleFor(x => x.Banco).NotEmpty().WithMessage("El banco es obligatorio.").MaximumLength(100);
        RuleFor(x => x.Importe).GreaterThan(0).WithMessage("El importe debe ser mayor a 0.");
        RuleFor(x => x.MonedaId).GreaterThan(0).WithMessage("La moneda es obligatoria.");
        RuleFor(x => x.FechaVencimiento)
            .GreaterThanOrEqualTo(x => x.FechaEmision)
            .WithMessage("La fecha de vencimiento no puede ser anterior a la de emisión.");
    }
}