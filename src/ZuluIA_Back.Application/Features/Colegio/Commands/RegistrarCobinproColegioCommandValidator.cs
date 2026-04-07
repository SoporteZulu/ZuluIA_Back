using FluentValidation;

namespace ZuluIA_Back.Application.Features.Colegio.Commands;

public class RegistrarCobinproColegioCommandValidator : AbstractValidator<RegistrarCobinproColegioCommand>
{
    public RegistrarCobinproColegioCommandValidator()
    {
        RuleFor(x => x.CedulonId).GreaterThan(0);
        RuleFor(x => x.Importe).GreaterThan(0);
        RuleFor(x => x.ReferenciaExterna).NotEmpty().MaximumLength(100);
        RuleFor(x => x.CajaId).GreaterThan(0);
        RuleFor(x => x.FormaPagoId).GreaterThan(0);
        RuleFor(x => x.MonedaId).GreaterThan(0);
        RuleFor(x => x.Cotizacion).GreaterThan(0);
    }
}
