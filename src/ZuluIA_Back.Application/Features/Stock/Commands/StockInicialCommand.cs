using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Stock.Commands;

public record StockInicialItemDto(
    long ItemId,
    long DepositoId,
    decimal Cantidad);

public record StockInicialCommand(
    IReadOnlyList<StockInicialItemDto> Items,
    string? Observacion
) : IRequest<Result<int>>;