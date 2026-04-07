using FluentValidation;

namespace ZuluIA_Back.Application.Features.PuntoVenta.Commands;

public class RegistrarComprobanteFiscalAlternativoCommandValidator : AbstractValidator<RegistrarComprobanteFiscalAlternativoCommand>
{
    public RegistrarComprobanteFiscalAlternativoCommandValidator()
    {
        RuleFor(x => x.SucursalId).GreaterThan(0);
        RuleFor(x => x.PuntoFacturacionId).GreaterThan(0);
        RuleFor(x => x.TipoComprobanteId).GreaterThan(0);
        RuleFor(x => x.TerceroId).GreaterThan(0);
        RuleFor(x => x.MonedaId).GreaterThan(0);
        RuleFor(x => x.Cotizacion).GreaterThan(0);
        RuleFor(x => x.Items).NotEmpty();
        RuleFor(x => x.ReferenciaExterna)
            .NotEmpty()
            .When(x => x.Flujo == Enums.TipoFlujoFiscalAlternativo.Deuce);
    }
}
