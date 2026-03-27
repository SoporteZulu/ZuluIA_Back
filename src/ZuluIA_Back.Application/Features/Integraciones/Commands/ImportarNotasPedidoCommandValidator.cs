using FluentValidation;

namespace ZuluIA_Back.Application.Features.Integraciones.Commands;

public class ImportarNotasPedidoCommandValidator : AbstractValidator<ImportarNotasPedidoCommand>
{
    public ImportarNotasPedidoCommandValidator()
    {
        RuleFor(x => x.NotasPedido).NotEmpty();
        RuleForEach(x => x.NotasPedido).ChildRules(nota =>
        {
            nota.RuleFor(x => x.ReferenciaExterna).NotEmpty();
            nota.RuleFor(x => x.SucursalId).GreaterThan(0);
            nota.RuleFor(x => x.TipoComprobanteId).GreaterThan(0);
            nota.RuleFor(x => x.TerceroId).GreaterThan(0);
            nota.RuleFor(x => x.MonedaId).GreaterThan(0);
            nota.RuleFor(x => x.Cotizacion).GreaterThan(0);
            nota.RuleFor(x => x.Items).NotEmpty();
        });
    }
}
