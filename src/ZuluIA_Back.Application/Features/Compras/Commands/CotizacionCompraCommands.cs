using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Compras.Commands;

public record CrearCotizacionItemDto(
    long? ItemId, string Descripcion, decimal Cantidad, decimal PrecioUnitario);

public record CrearCotizacionCompraCommand(
    long SucursalId,
    long? RequisicionId,
    long ProveedorId,
    DateOnly Fecha,
    DateOnly? FechaVencimiento,
    string? Observacion,
    IReadOnlyList<CrearCotizacionItemDto> Items)
    : IRequest<Result<long>>;

public record AceptarCotizacionCompraCommand(long Id) : IRequest<Result>;

public record RechazarCotizacionCompraCommand(long Id) : IRequest<Result>;
