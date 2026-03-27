using MediatR;
using ZuluIA_Back.Application.Features.NotasPedido.DTOs;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.NotasPedido.Queries;

public record GetNotasPedidoPagedQuery(
    int Page, int PageSize,
    long? SucursalId, long? TerceroId, string? Estado)
    : IRequest<PagedResult<NotaPedidoListDto>>;

public record GetNotaPedidoDetalleQuery(long Id) : IRequest<NotaPedidoDto?>;

public record GetNotasPedidoPendientesQuery(long SucursalId) : IRequest<IReadOnlyList<NotaPedidoListDto>>;

public class GetNotasPedidoPagedQueryHandler(INotaPedidoRepository repo)
    : IRequestHandler<GetNotasPedidoPagedQuery, PagedResult<NotaPedidoListDto>>
{
    public async Task<PagedResult<NotaPedidoListDto>> Handle(
        GetNotasPedidoPagedQuery request, CancellationToken ct)
    {
        var paged = await repo.GetPagedAsync(
            request.Page, request.PageSize,
            request.SucursalId, request.TerceroId, request.Estado, ct);

        var items = paged.Items.Select(np => MapToList(np)).ToList();
        return new PagedResult<NotaPedidoListDto>(items, paged.TotalCount, request.Page, request.PageSize);
    }

    private static NotaPedidoListDto MapToList(Domain.Entities.Ventas.NotaPedido np) => new()
    {
        Id               = np.Id,
        SucursalId       = np.SucursalId,
        TerceroId        = np.TerceroId,
        TerceroRazonSocial = string.Empty,
        Fecha            = np.Fecha,
        FechaVencimiento = np.FechaVencimiento,
        Total            = np.Total,
        Estado           = np.Estado.ToString(),
        VendedorId       = np.VendedorId,
        CreatedAt        = np.CreatedAt
    };
}

public class GetNotaPedidoDetalleQueryHandler(INotaPedidoRepository repo)
    : IRequestHandler<GetNotaPedidoDetalleQuery, NotaPedidoDto?>
{
    public async Task<NotaPedidoDto?> Handle(
        GetNotaPedidoDetalleQuery request, CancellationToken ct)
    {
        var np = await repo.GetByIdConItemsAsync(request.Id, ct);
        if (np is null) return null;

        return new NotaPedidoDto
        {
            Id               = np.Id,
            SucursalId       = np.SucursalId,
            TerceroId        = np.TerceroId,
            TerceroRazonSocial = string.Empty,
            Fecha            = np.Fecha,
            FechaVencimiento = np.FechaVencimiento,
            Total            = np.Total,
            Estado           = np.Estado.ToString(),
            VendedorId       = np.VendedorId,
            Observacion      = np.Observacion,
            CreatedAt        = np.CreatedAt,
            Items            = np.Items.Select(i => new NotaPedidoItemDto
            {
                Id               = i.Id,
                ItemId           = i.ItemId,
                Cantidad         = i.Cantidad,
                CantidadPendiente = i.CantidadPendiente,
                PrecioUnitario   = i.PrecioUnitario,
                Bonificacion     = i.Bonificacion,
                Subtotal         = i.Subtotal
            }).ToList()
        };
    }
}

public class GetNotasPedidoPendientesQueryHandler(INotaPedidoRepository repo)
    : IRequestHandler<GetNotasPedidoPendientesQuery, IReadOnlyList<NotaPedidoListDto>>
{
    public async Task<IReadOnlyList<NotaPedidoListDto>> Handle(
        GetNotasPedidoPendientesQuery request, CancellationToken ct)
    {
        var list = await repo.GetPendientesAsync(request.SucursalId, ct);
        return list.Select(np => new NotaPedidoListDto
        {
            Id               = np.Id,
            SucursalId       = np.SucursalId,
            TerceroId        = np.TerceroId,
            TerceroRazonSocial = string.Empty,
            Fecha            = np.Fecha,
            FechaVencimiento = np.FechaVencimiento,
            Total            = np.Total,
            Estado           = np.Estado.ToString(),
            VendedorId       = np.VendedorId,
            CreatedAt        = np.CreatedAt
        }).ToList();
    }
}
