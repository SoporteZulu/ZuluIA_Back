using FluentValidation;

namespace ZuluIA_Back.Application.Features.Impresion.Commands;

public class ImprimirComprobanteFiscalCommandValidator : AbstractValidator<ImprimirComprobanteFiscalCommand>
{
    public ImprimirComprobanteFiscalCommandValidator()
    {
        RuleFor(x => x.ComprobanteId).GreaterThan(0);
    }
}
