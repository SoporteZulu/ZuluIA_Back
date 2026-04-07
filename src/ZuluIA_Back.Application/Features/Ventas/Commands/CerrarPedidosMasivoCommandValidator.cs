using FluentValidation;

namespace ZuluIA_Back.Application.Features.Ventas.Commands;

public class CerrarPedidosMasivoCommandValidator : AbstractValidator<CerrarPedidosMasivoCommand>
{
    public CerrarPedidosMasivoCommandValidator()
    {
        RuleFor(x => x.MotivoCierre).MaximumLength(500);

        RuleFor(x => x)
            .Must(x => x.SucursalId.HasValue
                || x.TerceroId.HasValue
                || x.FechaDesde.HasValue
                || x.FechaHasta.HasValue
                || x.FechaEntregaDesde.HasValue
                || x.FechaEntregaHasta.HasValue
                || x.SoloPendientes)
            .WithMessage("Debe indicar al menos un filtro para el cierre masivo de pedidos.");
    }
}
