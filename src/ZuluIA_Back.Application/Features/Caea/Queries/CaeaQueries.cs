using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Caea.DTOs;
using ZuluIA_Back.Domain.Entities.Facturacion;
using ZuluIA_Back.Domain.Enums;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Caea.Queries;

public record GetCaeasPagedQuery(int Page, int PageSize, long? PuntoFacturacionId, string? Estado)
    : IRequest<IReadOnlyList<CaeaListDto>>;

public record GetCaeaDetalleQuery(long Id) : IRequest<CaeaListDto?>;

public class GetCaeasPagedQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetCaeasPagedQuery, IReadOnlyList<CaeaListDto>>
{
    public async Task<IReadOnlyList<CaeaListDto>> Handle(
        GetCaeasPagedQuery request, CancellationToken ct)
    {
        var query = db.Caeas.AsNoTracking();
        if (request.PuntoFacturacionId.HasValue)
            query = query.Where(x => x.PuntoFacturacionId == request.PuntoFacturacionId.Value);
        if (!string.IsNullOrEmpty(request.Estado) && Enum.TryParse<EstadoCaea>(request.Estado, true, out var estado))
            query = query.Where(x => x.Estado == estado);

        return await query
            .OrderByDescending(x => x.FechaDesde)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(c => new CaeaListDto
            {
                Id                 = c.Id,
                PuntoFacturacionId = c.PuntoFacturacionId,
                NroCaea            = c.NroCaea,
                FechaDesde         = c.FechaDesde,
                FechaHasta         = c.FechaHasta,
                FechaProcesoAfip   = c.FechaProcesoAfip,
                FechaTopeInformarAfip = c.FechaTopeInformarAfip,
                TipoComprobante    = c.TipoComprobante,
                CantidadAsignada   = c.CantidadAsignada,
                CantidadUsada      = c.CantidadUsada,
                Estado             = c.Estado.ToString(),
                CreatedAt          = c.CreatedAt
            }).ToListAsync(ct);
    }
}

public class GetCaeaDetalleQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetCaeaDetalleQuery, CaeaListDto?>
{
    public async Task<CaeaListDto?> Handle(GetCaeaDetalleQuery request, CancellationToken ct)
    {
        return await db.Caeas.AsNoTracking()
            .Where(x => x.Id == request.Id)
            .Select(c => new CaeaListDto
            {
                Id                 = c.Id,
                PuntoFacturacionId = c.PuntoFacturacionId,
                NroCaea            = c.NroCaea,
                FechaDesde         = c.FechaDesde,
                FechaHasta         = c.FechaHasta,
                FechaProcesoAfip   = c.FechaProcesoAfip,
                FechaTopeInformarAfip = c.FechaTopeInformarAfip,
                TipoComprobante    = c.TipoComprobante,
                CantidadAsignada   = c.CantidadAsignada,
                CantidadUsada      = c.CantidadUsada,
                Estado             = c.Estado.ToString(),
                CreatedAt          = c.CreatedAt
            }).FirstOrDefaultAsync(ct);
    }
}
