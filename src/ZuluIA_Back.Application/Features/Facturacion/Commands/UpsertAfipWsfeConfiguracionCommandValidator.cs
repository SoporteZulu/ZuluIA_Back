using FluentValidation;

namespace ZuluIA_Back.Application.Features.Facturacion.Commands;

public class UpsertAfipWsfeConfiguracionCommandValidator : AbstractValidator<UpsertAfipWsfeConfiguracionCommand>
{
    public UpsertAfipWsfeConfiguracionCommandValidator()
    {
        RuleFor(x => x.PuntoFacturacionId).GreaterThan(0);
        RuleFor(x => x.CuitEmisor).NotEmpty().MaximumLength(20);
    }
}
