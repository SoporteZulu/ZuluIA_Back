using MediatR;
using ZuluIA_Back.Application.Features.Stock.DTOs;

namespace ZuluIA_Back.Application.Features.Stock.Queries;

public record GetStockByDepositoQuery(long DepositoId) : IRequest<IReadOnlyList<StockItemDto>>;