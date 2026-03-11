using MediatR;
using ZuluIA_Back.Application.Features.Stock.DTOs;

namespace ZuluIA_Back.Application.Features.Stock.Queries;

public record GetStockByItemQuery(long ItemId) : IRequest<StockResumenDto?>;