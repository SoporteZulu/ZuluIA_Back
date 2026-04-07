using FluentValidation;

namespace ZuluIA_Back.Application.Features.Ventas.Commands;

public class ConvertirDocumentoVentaCommandValidator : AbstractValidator<ConvertirDocumentoVentaCommand>
{
    public ConvertirDocumentoVentaCommandValidator()
    {
        RuleFor(x => x.ComprobanteOrigenId).GreaterThan(0);
        RuleFor(x => x.TipoComprobanteDestinoId).GreaterThan(0);
    }
}
