using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Cobros.Commands;

public record CreateCobroMedioDto(
    long CajaId,
    long FormaPagoId,
    decimal Importe,
    long MonedaId,
    decimal Cotizacion,
    long? ChequeId = null
);

public record CreateCobroCommand(
    long SucursalId,
    long TerceroId,
    DateOnly Fecha,
    long MonedaId,
    decimal Cotizacion,
    string? Observacion,
    List<CreateCobroMedioDto> Medios
) : IRequest<Result<long>>;