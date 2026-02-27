using MediatR;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Application.Features.Pagos.DTOs;

namespace ZuluIA_Back.Application.Features.Pagos.Commands;

public record CreatePagoCommand(
    long SucursalId,
    long TerceroId,
    DateOnly Fecha,
    long MonedaId,
    decimal Cotizacion,
    string? Observacion,
    List<CreatePagoMedioDto> Medios
) : IRequest<Result<long>>;