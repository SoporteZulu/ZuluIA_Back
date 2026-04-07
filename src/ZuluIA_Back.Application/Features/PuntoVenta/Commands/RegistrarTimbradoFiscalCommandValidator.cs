using FluentValidation;

namespace ZuluIA_Back.Application.Features.PuntoVenta.Commands;

public class RegistrarTimbradoFiscalCommandValidator : AbstractValidator<RegistrarTimbradoFiscalCommand>
{
    public RegistrarTimbradoFiscalCommandValidator()
    {
        RuleFor(x => x.SucursalId).GreaterThan(0);
        RuleFor(x => x.PuntoFacturacionId).GreaterThan(0);
        RuleFor(x => x.NumeroTimbrado).NotEmpty().MaximumLength(50);
        RuleFor(x => x.VigenciaHasta).GreaterThanOrEqualTo(x => x.VigenciaDesde);
    }
}
