using FluentValidation;

namespace ZuluIA_Back.Application.Features.Colegio.Commands;

public class CancelarDeudaColegioCommandValidator : AbstractValidator<CancelarDeudaColegioCommand>
{
    public CancelarDeudaColegioCommandValidator()
    {
        RuleFor(x => x.SucursalId).GreaterThan(0);
        RuleFor(x => x.TerceroId).GreaterThan(0);
        RuleFor(x => x.CajaId).GreaterThan(0);
        RuleFor(x => x.FormaPagoId).GreaterThan(0);
        RuleFor(x => x.MonedaId).GreaterThan(0);
        RuleFor(x => x.Cotizacion).GreaterThan(0);
        RuleFor(x => x.Cedulones).NotEmpty();
        RuleForEach(x => x.Cedulones).ChildRules(c =>
        {
            c.RuleFor(x => x.CedulonId).GreaterThan(0);
            c.RuleFor(x => x.Importe).GreaterThan(0);
        });
    }
}
