using MediatR;
using ZuluIA_Back.Application.Features.Stock.DTOs;

namespace ZuluIA_Back.Application.Features.Stock.Queries;

public record GetStockBajoMinimoQuery(
    long? SucursalId,
    long? DepositoId)
    : IRequest<IReadOnlyList<StockBajoMinimoDto>>;