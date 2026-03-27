using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Tesoreria.Commands;

public record RegistrarReintegroTesoreriaCommand(
    long SucursalId,
    long CajaCuentaId,
    DateOnly Fecha,
    decimal Importe,
    long MonedaId,
    decimal Cotizacion,
    long? TerceroId,
    string? Observacion
) : IRequest<Result<long>>;
