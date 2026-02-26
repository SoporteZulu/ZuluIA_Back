using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Pagos.Commands;

public record CreatePagoMedioDto(
    long CajaId,
    long FormaPagoId,
    decimal Importe,
    long MonedaId,
    decimal Cotizacion,
    long? ChequeId = null
);

public record CreatePagoCommand(
    long SucursalId,
    long TerceroId,
    DateOnly Fecha,
    long MonedaId,
    decimal Cotizacion,
    string? Observacion,
    List<CreatePagoMedioDto> Medios
) : IRequest<Result<long>>;