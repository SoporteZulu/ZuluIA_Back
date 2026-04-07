using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Tesoreria.Commands;

public record RegistrarDepositoOperarCommand(
    long SucursalId,
    long CajaOrigenId,
    long CajaDestinoId,
    DateOnly Fecha,
    decimal Importe,
    long MonedaId,
    decimal Cotizacion,
    string? Observacion
) : IRequest<Result<IReadOnlyList<long>>>;
