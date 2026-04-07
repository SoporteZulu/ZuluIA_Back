using FluentValidation;

namespace ZuluIA_Back.Application.Features.PuntoVenta.Commands;

public class UpdateTimbradoFiscalCommandValidator : AbstractValidator<UpdateTimbradoFiscalCommand>
{
    public UpdateTimbradoFiscalCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.NumeroTimbrado).NotEmpty().MaximumLength(50);
        RuleFor(x => x.VigenciaHasta).GreaterThanOrEqualTo(x => x.VigenciaDesde);
    }
}
