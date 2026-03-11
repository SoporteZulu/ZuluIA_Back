using FluentValidation;

namespace ZuluIA_Back.Application.Features.Comprobantes.Commands;

public class ImputarComprobanteCommandValidator
    : AbstractValidator<ImputarComprobanteCommand>
{
    public ImputarComprobanteCommandValidator()
    {
        RuleFor(x => x.ComprobanteOrigenId)
            .GreaterThan(0).WithMessage("El comprobante origen es obligatorio.");

        RuleFor(x => x.ComprobanteDestinoId)
            .GreaterThan(0).WithMessage("El comprobante destino es obligatorio.")
            .NotEqual(x => x.ComprobanteOrigenId)
            .WithMessage("El comprobante destino no puede ser igual al origen.");

        RuleFor(x => x.Importe)
            .GreaterThan(0).WithMessage("El importe debe ser mayor a 0.");

        RuleFor(x => x.Fecha)
            .NotEmpty().WithMessage("La fecha es obligatoria.");
    }
}