using FluentValidation;

namespace ZuluIA_Back.Application.Features.Compras.Commands;

public class EmitirDocumentoCompraCommandValidator : AbstractValidator<EmitirDocumentoCompraCommand>
{
    public EmitirDocumentoCompraCommandValidator()
    {
        RuleFor(x => x.ComprobanteId).GreaterThan(0);
    }
}
