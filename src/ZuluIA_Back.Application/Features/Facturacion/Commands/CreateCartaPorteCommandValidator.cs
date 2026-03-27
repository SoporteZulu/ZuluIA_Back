using FluentValidation;

namespace ZuluIA_Back.Application.Features.Facturacion.Commands;

public class CreateCartaPorteCommandValidator : AbstractValidator<CreateCartaPorteCommand>
{
    public CreateCartaPorteCommandValidator()
    {
        RuleFor(x => x.CuitRemitente).NotEmpty().MaximumLength(20);
        RuleFor(x => x.CuitDestinatario).NotEmpty().MaximumLength(20);
    }
}
