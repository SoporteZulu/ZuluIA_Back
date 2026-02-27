using MediatR;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Application.Features.Comprobantes.DTOs;

namespace ZuluIA_Back.Application.Features.Comprobantes.Commands;

public record CreateComprobanteCommand(
    long SucursalId,
    long? PuntoFacturacionId,
    long TipoComprobanteId,
    short Prefijo,
    long Numero,
    DateOnly Fecha,
    DateOnly? FechaVencimiento,
    long TerceroId,
    long MonedaId,
    decimal Cotizacion,
    string? Observacion,
    List<CreateComprobanteItemDto> Items
) : IRequest<Result<long>>;