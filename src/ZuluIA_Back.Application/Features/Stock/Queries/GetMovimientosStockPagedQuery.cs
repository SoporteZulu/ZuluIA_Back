using MediatR;
using ZuluIA_Back.Application.Features.Stock.DTOs;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Application.Features.Stock.Queries;

public record GetMovimientosStockPagedQuery(
    int Page,
    int PageSize,
    long? ItemId,
    long? DepositoId,
    TipoMovimientoStock? Tipo,
    DateOnly? Desde,
    DateOnly? Hasta)
    : IRequest<PagedResult<MovimientoStockDto>>;