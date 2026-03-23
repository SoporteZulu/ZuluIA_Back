using MediatR;
using ZuluIA_Back.Application.Features.Compras.DTOs;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Compras.Queries;

public record GetCotizacionesCompraPagedQuery(
    int Page, int PageSize,
    long? SucursalId, long? ProveedorId, string? Estado)
    : IRequest<PagedResult<CotizacionCompraListDto>>;

public record GetCotizacionCompraDetalleQuery(long Id) : IRequest<CotizacionCompraDto?>;

public class GetCotizacionesCompraPagedQueryHandler(ICotizacionCompraRepository repo)
    : IRequestHandler<GetCotizacionesCompraPagedQuery, PagedResult<CotizacionCompraListDto>>
{
    public async Task<PagedResult<CotizacionCompraListDto>> Handle(
        GetCotizacionesCompraPagedQuery request, CancellationToken ct)
    {
        var paged = await repo.GetPagedAsync(
            request.Page, request.PageSize,
            request.SucursalId, request.ProveedorId, request.Estado, ct);

        var items = paged.Items.Select(c => new CotizacionCompraListDto
        {
            Id                  = c.Id,
            SucursalId          = c.SucursalId,
            RequisicionId       = c.RequisicionId,
            ProveedorId         = c.ProveedorId,
            ProveedorRazonSocial = string.Empty,
            Fecha               = c.Fecha,
            FechaVencimiento    = c.FechaVencimiento,
            Total               = c.Total,
            Estado              = c.Estado.ToString(),
            CreatedAt           = c.CreatedAt
        }).ToList();

        return new PagedResult<CotizacionCompraListDto>(items, paged.TotalCount, request.Page, request.PageSize);
    }
}

public class GetCotizacionCompraDetalleQueryHandler(ICotizacionCompraRepository repo)
    : IRequestHandler<GetCotizacionCompraDetalleQuery, CotizacionCompraDto?>
{
    public async Task<CotizacionCompraDto?> Handle(
        GetCotizacionCompraDetalleQuery request, CancellationToken ct)
    {
        var c = await repo.GetByIdConItemsAsync(request.Id, ct);
        if (c is null) return null;

        return new CotizacionCompraDto
        {
            Id                  = c.Id,
            SucursalId          = c.SucursalId,
            RequisicionId       = c.RequisicionId,
            ProveedorId         = c.ProveedorId,
            ProveedorRazonSocial = string.Empty,
            Fecha               = c.Fecha,
            FechaVencimiento    = c.FechaVencimiento,
            Total               = c.Total,
            Estado              = c.Estado.ToString(),
            Observacion         = c.Observacion,
            CreatedAt           = c.CreatedAt,
            Items               = c.Items.Select(i => new CotizacionCompraItemDto
            {
                Id             = i.Id,
                ItemId         = i.ItemId,
                Descripcion    = i.Descripcion,
                Cantidad       = i.Cantidad,
                PrecioUnitario = i.PrecioUnitario,
                Total          = i.Total
            }).ToList()
        };
    }
}
