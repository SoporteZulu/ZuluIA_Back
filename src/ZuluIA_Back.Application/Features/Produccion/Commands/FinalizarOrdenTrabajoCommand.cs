using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Produccion.Commands;

public record FinalizarOrdenTrabajoCommand(
    long Id,
    DateOnly FechaFinReal
) : IRequest<Result>;