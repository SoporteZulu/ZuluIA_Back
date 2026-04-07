using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Produccion.Commands;

public record CrearOrdenEmpaqueCommand(
    long OrdenTrabajoId,
    long ItemId,
    long DepositoId,
    DateOnly Fecha,
    decimal Cantidad,
    string? Lote,
    string? Observacion
) : IRequest<Result<long>>;
