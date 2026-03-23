using FluentValidation;

namespace ZuluIA_Back.Application.Features.Facturacion.Commands;

public class CreateCartaPorteCommandValidator : AbstractValidator<CreateCartaPorteCommand>
{
    public CreateCartaPorteCommandValidator()
    {
        RuleFor(x => x.CuitRemitente).NotEmpty().WithMessage("El CUIT del remitente es obligatorio.")
            .Length(11).WithMessage("El CUIT debe tener 11 dígitos.");
        RuleFor(x => x.CuitDestinatario).NotEmpty().WithMessage("El CUIT del destinatario es obligatorio.")
            .Length(11).WithMessage("El CUIT debe tener 11 dígitos.");
        RuleFor(x => x.CuitTransportista).Length(11).When(x => x.CuitTransportista != null)
            .WithMessage("El CUIT del transportista debe tener 11 dígitos.");
        RuleFor(x => x.FechaEmision).NotEmpty().WithMessage("La fecha de emisión es obligatoria.");
    }
}
