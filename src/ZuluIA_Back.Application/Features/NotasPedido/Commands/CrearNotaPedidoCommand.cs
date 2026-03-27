using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.NotasPedido.Commands;

public record CrearNotaPedidoItemDto(
    long ItemId, decimal Cantidad, decimal PrecioUnitario, decimal Bonificacion);

public record CrearNotaPedidoCommand(
    long SucursalId,
    long TerceroId,
    DateOnly Fecha,
    DateOnly? FechaVencimiento,
    string? Observacion,
    long? VendedorId,
    IReadOnlyList<CrearNotaPedidoItemDto> Items)
    : IRequest<Result<long>>;
