using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Comprobantes.DTOs;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Comprobantes.Queries;

public class GetComprobantesPagedQueryHandler(
    IComprobanteRepository repo,
    IApplicationDbContext db)
    : IRequestHandler<GetComprobantesPagedQuery, PagedResult<ComprobanteListDto>>
{
    public async Task<PagedResult<ComprobanteListDto>> Handle(
        GetComprobantesPagedQuery request,
        CancellationToken ct)
    {
        var result = await repo.GetPagedAsync(
            request.Page, request.PageSize,
            request.SucursalId, request.TerceroId,
            request.TipoComprobanteId, request.Estado,
            request.Desde, request.Hasta, ct);

        // Lookup de descripciones y símbolos
        var terceroIds = result.Items.Select(x => x.TerceroId).Distinct().ToList();
        var tipoIds = result.Items.Select(x => x.TipoComprobanteId).Distinct().ToList();
        var monedaIds = result.Items.Select(x => x.MonedaId).Distinct().ToList();

        var terceros = await db.Terceros
            .AsNoTracking()
            .Where(x => terceroIds.Contains(x.Id))
            .Select(x => new { x.Id, x.RazonSocial })
            .ToDictionaryAsync(x => x.Id, ct);

        var tipos = await db.TiposComprobante
            .AsNoTracking()
            .Where(x => tipoIds.Contains(x.Id))
            .Select(x => new { x.Id, x.Descripcion })
            .ToDictionaryAsync(x => x.Id, ct);

        var monedas = await db.Monedas
            .AsNoTracking()
            .Where(x => monedaIds.Contains(x.Id))
            .Select(x => new { x.Id, x.Simbolo })
            .ToDictionaryAsync(x => x.Id, ct);

        var dtos = result.Items.Select(c => new ComprobanteListDto
        {
            Id                          = c.Id,
            SucursalId                  = c.SucursalId,
            TipoComprobanteId           = c.TipoComprobanteId,
            TipoComprobanteDescripcion  = tipos.GetValueOrDefault(c.TipoComprobanteId)?.Descripcion ?? "—",
            Prefijo                     = c.Numero.Prefijo,
            Numero                      = c.Numero.Numero,
            NumeroFormateado            = c.Numero.Formateado,
            Fecha                       = c.Fecha,
            FechaVencimiento            = c.FechaVencimiento,
            TerceroId                   = c.TerceroId,
            TerceroRazonSocial          = terceros.GetValueOrDefault(c.TerceroId)?.RazonSocial ?? "—",
            MonedaId                    = c.MonedaId,
            MonedaSimbolo               = monedas.GetValueOrDefault(c.MonedaId)?.Simbolo ?? "$",
            Total                       = c.Total,
            Saldo                       = c.Saldo,
            Estado                      = c.Estado,
            Cae                         = c.Cae,
            TieneCae                    = !string.IsNullOrEmpty(c.Cae)
        }).ToList();

        return new PagedResult<ComprobanteListDto>(
            dtos, result.Page, result.PageSize, result.TotalCount);
    }
}
