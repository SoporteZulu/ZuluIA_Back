using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Produccion.Commands;

public record FinalizarOrdenTrabajoCommand(
    long Id,
    DateOnly FechaFinReal,
    decimal? CantidadProducida = null,
    IReadOnlyList<ConsumoOrdenTrabajoInput>? Consumos = null
) : IRequest<Result>;