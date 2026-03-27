using FluentValidation;

namespace ZuluIA_Back.Application.Features.PuntoVenta.Commands;

public class DesactivarTimbradoFiscalCommandValidator : AbstractValidator<DesactivarTimbradoFiscalCommand>
{
    public DesactivarTimbradoFiscalCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
