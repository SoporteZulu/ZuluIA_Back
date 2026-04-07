using FluentValidation;

namespace ZuluIA_Back.Application.Features.Facturacion.Commands;

public class EjecutarFacturacionAutomaticaCommandValidator : AbstractValidator<EjecutarFacturacionAutomaticaCommand>
{
    public EjecutarFacturacionAutomaticaCommandValidator()
    {
        RuleFor(x => x.SucursalId).GreaterThan(0);
        RuleFor(x => x.TipoComprobanteOrigenId).GreaterThan(0);
        RuleFor(x => x.TipoComprobanteDestinoId).GreaterThan(0);
        RuleFor(x => x.Hasta).GreaterThanOrEqualTo(x => x.Desde);
    }
}
