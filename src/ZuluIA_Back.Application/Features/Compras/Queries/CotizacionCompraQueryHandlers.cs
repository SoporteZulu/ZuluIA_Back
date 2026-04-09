using MediatR;
using ZuluIA_Back.Application.Common.Extensions;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Compras.DTOs;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Compras.Queries;

public record GetCotizacionesCompraPagedQuery(
    int Page, int PageSize,
    long? SucursalId, long? ProveedorId, string? Estado)
    : IRequest<PagedResult<CotizacionCompraListDto>>;

public record GetCotizacionCompraDetalleQuery(long Id) : IRequest<CotizacionCompraDto?>;

public class GetCotizacionesCompraPagedQueryHandler(ICotizacionCompraRepository repo, IApplicationDbContext db)
    : IRequestHandler<GetCotizacionesCompraPagedQuery, PagedResult<CotizacionCompraListDto>>
{
    public async Task<PagedResult<CotizacionCompraListDto>> Handle(
        GetCotizacionesCompraPagedQuery request, CancellationToken ct)
    {
        var paged = await repo.GetPagedAsync(
            request.Page, request.PageSize,
            request.SucursalId, request.ProveedorId, request.Estado, ct);

        var proveedorIds = paged.Items.Select(x => x.ProveedorId).Distinct().ToList();
        var proveedores = await db.Terceros
            .AsNoTrackingSafe()
            .Where(x => proveedorIds.Contains(x.Id))
            .Select(x => new { x.Id, x.RazonSocial })
            .ToDictionarySafeAsync(x => x.Id, x => x.RazonSocial, ct);

        var items = paged.Items.Select(c => new CotizacionCompraListDto
        {
            Id                  = c.Id,
            SucursalId          = c.SucursalId,
            RequisicionId       = c.RequisicionId,
            ProveedorId         = c.ProveedorId,
            ProveedorRazonSocial = proveedores.GetValueOrDefault(c.ProveedorId, string.Empty),
            RequisicionReferencia = c.RequisicionId.HasValue ? $"REQ-{c.RequisicionId.Value}" : null,
            Fecha               = c.Fecha,
            FechaVencimiento    = c.FechaVencimiento,
            Total               = c.Total,
            Estado              = c.Estado.ToString(),
            EstadoLegacy        = CotizacionCompraQueryMappings.MapEstadoCotizacionLegacy(c.Estado),
            CantidadItems       = c.Items.Count,
            CreatedAt           = c.CreatedAt
        }).ToList();

        return new PagedResult<CotizacionCompraListDto>(items, request.Page, request.PageSize, paged.TotalCount);
    }
}

public class GetCotizacionCompraDetalleQueryHandler(ICotizacionCompraRepository repo, IApplicationDbContext db)
    : IRequestHandler<GetCotizacionCompraDetalleQuery, CotizacionCompraDto?>
{
    public async Task<CotizacionCompraDto?> Handle(
        GetCotizacionCompraDetalleQuery request, CancellationToken ct)
    {
        var c = await repo.GetByIdConItemsAsync(request.Id, ct);
        if (c is null) return null;

        var proveedorNombre = await db.Terceros
            .AsNoTrackingSafe()
            .Where(x => x.Id == c.ProveedorId)
            .Select(x => x.RazonSocial)
            .FirstOrDefaultSafeAsync(ct)
            ?? string.Empty;

        var itemIds = c.Items.Where(x => x.ItemId.HasValue).Select(x => x.ItemId!.Value).Distinct().ToList();
        var codigos = await db.Items
            .AsNoTrackingSafe()
            .Where(x => itemIds.Contains(x.Id))
            .Select(x => new { x.Id, x.Codigo })
            .ToDictionarySafeAsync(x => x.Id, x => x.Codigo, ct);

        return new CotizacionCompraDto
        {
            Id                  = c.Id,
            SucursalId          = c.SucursalId,
            RequisicionId       = c.RequisicionId,
            ProveedorId         = c.ProveedorId,
            ProveedorRazonSocial = proveedorNombre,
            RequisicionReferencia = c.RequisicionId.HasValue ? $"REQ-{c.RequisicionId.Value}" : null,
            Fecha               = c.Fecha,
            FechaVencimiento    = c.FechaVencimiento,
            Total               = c.Total,
            Estado              = c.Estado.ToString(),
            EstadoLegacy        = CotizacionCompraQueryMappings.MapEstadoCotizacionLegacy(c.Estado),
            Observacion         = c.Observacion,
            CantidadItems       = c.Items.Count,
            CreatedAt           = c.CreatedAt,
            Items               = c.Items.Select(i => new CotizacionCompraItemDto
            {
                Id             = i.Id,
                ItemId         = i.ItemId,
                Codigo         = i.ItemId.HasValue ? codigos.GetValueOrDefault(i.ItemId.Value, string.Empty) : string.Empty,
                Descripcion    = i.Descripcion,
                Cantidad       = i.Cantidad,
                PrecioUnitario = i.PrecioUnitario,
                Total          = i.Total
            }).ToList()
        };
    }
}

internal static class CotizacionCompraQueryMappings
{
    public static string MapEstadoCotizacionLegacy(Domain.Enums.EstadoCotizacionCompra estado) => estado switch
    {
        Domain.Enums.EstadoCotizacionCompra.Pendiente => "ENVIADA",
        Domain.Enums.EstadoCotizacionCompra.Aceptada => "APROBADA",
        Domain.Enums.EstadoCotizacionCompra.Rechazada => "RECHAZADA",
        Domain.Enums.EstadoCotizacionCompra.Procesada => "APROBADA",
        _ => estado.ToString().ToUpperInvariant()
    };
}
