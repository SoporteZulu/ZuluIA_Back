using FluentValidation;

namespace ZuluIA_Back.Application.Features.PuntoVenta.Commands;

public class ProcesarComprobanteSifenCommandValidator : AbstractValidator<ProcesarComprobanteSifenCommand>
{
    public ProcesarComprobanteSifenCommandValidator()
    {
        RuleFor(x => x.ComprobanteId).GreaterThan(0);
    }
}
