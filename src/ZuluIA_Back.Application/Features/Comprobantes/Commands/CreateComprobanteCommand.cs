using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Comprobantes.Commands;

public record CreateComprobanteItemDto(
    long ItemId,
    string Descripcion,
    decimal Cantidad,
    decimal PrecioUnitario,
    decimal DescuentoPct,
    long AlicuotaIvaId,
    decimal PorcentajeIva,
    long? DepositoId,
    short Orden,
    decimal CantidadBonif = 0
);

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