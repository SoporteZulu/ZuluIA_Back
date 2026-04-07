using FluentValidation;

namespace ZuluIA_Back.Application.Features.Colegio.Commands;

public class CancelarDeudaColegioMasivaCommandValidator : AbstractValidator<CancelarDeudaColegioMasivaCommand>
{
    public CancelarDeudaColegioMasivaCommandValidator()
    {
        RuleFor(x => x.SucursalId).GreaterThan(0);
        RuleFor(x => x.CajaId).GreaterThan(0);
        RuleFor(x => x.FormaPagoId).GreaterThan(0);
        RuleFor(x => x.MonedaId).GreaterThan(0);
        RuleFor(x => x.Cotizacion).GreaterThan(0);
        RuleFor(x => x.Cedulones).NotEmpty();
    }
}
