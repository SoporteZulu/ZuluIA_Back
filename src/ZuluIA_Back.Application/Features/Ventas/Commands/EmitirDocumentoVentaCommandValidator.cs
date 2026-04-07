using FluentValidation;

namespace ZuluIA_Back.Application.Features.Ventas.Commands;

public class EmitirDocumentoVentaCommandValidator : AbstractValidator<EmitirDocumentoVentaCommand>
{
    public EmitirDocumentoVentaCommandValidator()
    {
        RuleFor(x => x.ComprobanteId).GreaterThan(0);
    }
}
