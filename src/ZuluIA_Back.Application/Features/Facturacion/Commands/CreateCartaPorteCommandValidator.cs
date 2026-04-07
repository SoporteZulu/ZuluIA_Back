using FluentValidation;

namespace ZuluIA_Back.Application.Features.Facturacion.Commands;

public class CreateCartaPorteCommandValidator : AbstractValidator<CreateCartaPorteCommand>
{
    public CreateCartaPorteCommandValidator()
    {
        RuleFor(x => x.CuitRemitente)
            .NotEmpty()
            .Length(11)
            .Matches("^[0-9]+$");

        RuleFor(x => x.CuitDestinatario)
            .NotEmpty()
            .Length(11)
            .Matches("^[0-9]+$");

        RuleFor(x => x.CuitTransportista)
            .Length(11)
            .Matches("^[0-9]+$")
            .When(x => !string.IsNullOrWhiteSpace(x.CuitTransportista));
    }
}
