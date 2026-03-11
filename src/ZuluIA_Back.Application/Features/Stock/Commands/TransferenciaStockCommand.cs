using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Stock.Commands;

public record TransferenciaStockCommand(
    long ItemId,
    long DepositoOrigenId,
    long DepositoDestinoId,
    decimal Cantidad,
    string? Observacion
) : IRequest<Result>;