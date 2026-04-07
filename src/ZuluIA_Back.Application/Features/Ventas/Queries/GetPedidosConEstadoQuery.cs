using MediatR;
using ZuluIA_Back.Application.Features.Ventas.DTOs;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Application.Features.Ventas.Queries;

public record GetPedidosConEstadoQuery(
    int Page = 1,
    int PageSize = 20,
    long? SucursalId = null,
    long? TerceroId = null,
    DateOnly? FechaDesde = null,
    DateOnly? FechaHasta = null,
    DateOnly? FechaEntregaDesde = null,
    DateOnly? FechaEntregaHasta = null,
    EstadoPedido? EstadoPedido = null,
    EstadoEntregaItem? EstadoEntregaItem = null,
    bool? SoloAtrasados = null,
    long? ItemId = null,
    string? CodigoOConcepto = null) : IRequest<PagedResult<PedidoConEstadoDto>>;
