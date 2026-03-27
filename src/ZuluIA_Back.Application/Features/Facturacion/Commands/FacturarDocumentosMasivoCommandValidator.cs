using FluentValidation;

namespace ZuluIA_Back.Application.Features.Facturacion.Commands;

public class FacturarDocumentosMasivoCommandValidator : AbstractValidator<FacturarDocumentosMasivoCommand>
{
    public FacturarDocumentosMasivoCommandValidator()
    {
        RuleFor(x => x.ComprobanteOrigenIds).NotEmpty();
        RuleFor(x => x.TipoComprobanteDestinoId).GreaterThan(0);
        RuleForEach(x => x.ComprobanteOrigenIds).GreaterThan(0);
    }
}
