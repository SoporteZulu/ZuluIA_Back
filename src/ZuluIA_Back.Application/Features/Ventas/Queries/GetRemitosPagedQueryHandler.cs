using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Comprobantes.DTOs;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Ventas.Queries;

public class GetRemitosPagedQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetRemitosPagedQuery, PagedResult<ComprobanteListDto>>
{
    public async Task<PagedResult<ComprobanteListDto>> Handle(GetRemitosPagedQuery request, CancellationToken ct)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;

        var remitoTipoIds = await db.TiposComprobante
            .AsNoTracking()
            .Where(x => x.Activo && x.EsVenta)
            .Where(x =>
                x.Descripcion.ToUpper().Contains("REMIT") ||
                x.Codigo.ToUpper().Contains("REMIT"))
            .Select(x => x.Id)
            .ToListAsync(ct);

        if (remitoTipoIds.Count == 0)
            return new PagedResult<ComprobanteListDto>([], page, pageSize, 0);

        var query = db.Comprobantes
            .AsNoTracking()
            .Where(x => remitoTipoIds.Contains(x.TipoComprobanteId));

        if (request.SucursalId.HasValue)
            query = query.Where(x => x.SucursalId == request.SucursalId.Value);

        if (request.FechaDesde.HasValue)
            query = query.Where(x => x.Fecha >= request.FechaDesde.Value);

        if (request.FechaHasta.HasValue)
            query = query.Where(x => x.Fecha <= request.FechaHasta.Value);

        if (request.Prefijo.HasValue)
            query = query.Where(x => x.Numero.Prefijo == request.Prefijo.Value);

        if (request.Numero.HasValue)
            query = query.Where(x => x.Numero.Numero == request.Numero.Value);

        if (request.DepositoId.HasValue)
            query = query.Where(x => x.DepositoOrigenId == request.DepositoId.Value);

        if (request.Estado.HasValue)
            query = query.Where(x => x.Estado == request.Estado.Value);

        if (request.EstadoLogistico.HasValue)
            query = query.Where(x => x.EstadoLogistico == request.EstadoLogistico.Value);

        if (request.EsValorizado.HasValue)
            query = query.Where(x => x.EsValorizado == request.EsValorizado.Value);

        if (!string.IsNullOrWhiteSpace(request.TerceroLegajo))
        {
            var legajo = request.TerceroLegajo.Trim();
            var terceroIdsPorLegajo = await db.Terceros
                .AsNoTracking()
                .Where(x => x.Legajo == legajo)
                .Select(x => x.Id)
                .ToListAsync(ct);

            query = query.Where(x => terceroIdsPorLegajo.Contains(x.TerceroId));
        }

        if (!string.IsNullOrWhiteSpace(request.TerceroDenominacionSocial))
        {
            var denominacion = request.TerceroDenominacionSocial.Trim().ToUpper();
            var terceroIdsPorNombre = await db.Terceros
                .AsNoTracking()
                .Where(x => x.RazonSocial.ToUpper().Contains(denominacion))
                .Select(x => x.Id)
                .ToListAsync(ct);

            query = query.Where(x => terceroIdsPorNombre.Contains(x.TerceroId));
        }

        if (!string.IsNullOrWhiteSpace(request.CotNumero))
        {
            var cotNumero = request.CotNumero.Trim().ToUpper();
            var comprobanteIdsConCot = await db.ComprobantesCot
                .AsNoTracking()
                .Where(x => x.Numero.ToUpper().Contains(cotNumero))
                .Select(x => x.ComprobanteId)
                .ToListAsync(ct);

            query = query.Where(x => comprobanteIdsConCot.Contains(x.Id));
        }

        if (request.CotFechaDesde.HasValue)
        {
            var comprobanteIdsConCot = await db.ComprobantesCot
                .AsNoTracking()
                .Where(x => x.FechaVigencia >= request.CotFechaDesde.Value)
                .Select(x => x.ComprobanteId)
                .ToListAsync(ct);

            query = query.Where(x => comprobanteIdsConCot.Contains(x.Id));
        }

        if (request.CotFechaHasta.HasValue)
        {
            var comprobanteIdsConCot = await db.ComprobantesCot
                .AsNoTracking()
                .Where(x => x.FechaVigencia <= request.CotFechaHasta.Value)
                .Select(x => x.ComprobanteId)
                .ToListAsync(ct);

            query = query.Where(x => comprobanteIdsConCot.Contains(x.Id));
        }

        var totalCount = await query.CountAsync(ct);

        var rows = await query
            .OrderByDescending(x => x.Fecha)
            .ThenByDescending(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new
            {
                x.Id,
                x.SucursalId,
                x.TipoComprobanteId,
                Prefijo = x.Numero.Prefijo,
                Numero = x.Numero.Numero,
                x.Fecha,
                x.FechaVencimiento,
                x.TerceroId,
                x.MonedaId,
                x.Total,
                x.Saldo,
                x.Estado,
                x.Cae,
                x.DepositoOrigenId,
                x.EstadoLogistico,
                x.EsValorizado
            })
            .ToListAsync(ct);

        var terceroIds = rows.Select(x => x.TerceroId).Distinct().ToList();
        var tipoComprobanteIds = rows.Select(x => x.TipoComprobanteId).Distinct().ToList();
        var monedaIds = rows.Select(x => x.MonedaId).Distinct().ToList();
        var depositoIds = rows.Where(x => x.DepositoOrigenId.HasValue).Select(x => x.DepositoOrigenId!.Value).Distinct().ToList();
        var comprobanteIds = rows.Select(x => x.Id).ToList();

        var terceros = await db.Terceros
            .AsNoTracking()
            .Where(x => terceroIds.Contains(x.Id))
            .Select(x => new { x.Id, x.RazonSocial, x.Legajo })
            .ToDictionaryAsync(x => x.Id, ct);

        var tipos = await db.TiposComprobante
            .AsNoTracking()
            .Where(x => tipoComprobanteIds.Contains(x.Id))
            .Select(x => new { x.Id, x.Descripcion })
            .ToDictionaryAsync(x => x.Id, ct);

        var monedas = await db.Monedas
            .AsNoTracking()
            .Where(x => monedaIds.Contains(x.Id))
            .Select(x => new { x.Id, x.Simbolo })
            .ToDictionaryAsync(x => x.Id, ct);

        var depositos = await db.Depositos
            .AsNoTracking()
            .Where(x => depositoIds.Contains(x.Id))
            .Select(x => new { x.Id, x.Descripcion })
            .ToDictionaryAsync(x => x.Id, ct);

        var cotPorComprobante = await db.ComprobantesCot
            .AsNoTracking()
            .Where(x => comprobanteIds.Contains(x.ComprobanteId))
            .Select(x => new { x.ComprobanteId, x.Numero, x.FechaVigencia })
            .ToDictionaryAsync(x => x.ComprobanteId, ct);

        var items = rows.Select(x => new ComprobanteListDto
        {
            Id = x.Id,
            SucursalId = x.SucursalId,
            SucursalCodigo = x.SucursalId.ToString(),
            TipoComprobanteId = x.TipoComprobanteId,
            TipoComprobanteDescripcion = tipos.GetValueOrDefault(x.TipoComprobanteId)?.Descripcion ?? "—",
            Prefijo = x.Prefijo,
            Numero = x.Numero,
            NumeroFormateado = $"{x.Prefijo:D4}-{x.Numero:D8}",
            Fecha = x.Fecha,
            FechaVencimiento = x.FechaVencimiento,
            TerceroId = x.TerceroId,
            TerceroRazonSocial = terceros.GetValueOrDefault(x.TerceroId)?.RazonSocial ?? "—",
            TerceroLegajo = terceros.GetValueOrDefault(x.TerceroId)?.Legajo,
            MonedaId = x.MonedaId,
            MonedaSimbolo = monedas.GetValueOrDefault(x.MonedaId)?.Simbolo ?? "$",
            DepositoOrigenId = x.DepositoOrigenId,
            DepositoDescripcion = x.DepositoOrigenId.HasValue
                ? depositos.GetValueOrDefault(x.DepositoOrigenId.Value)?.Descripcion
                : null,
            CotNumero = cotPorComprobante.GetValueOrDefault(x.Id)?.Numero,
            CotFechaVigencia = cotPorComprobante.GetValueOrDefault(x.Id)?.FechaVigencia,
            EstadoLogistico = x.EstadoLogistico,
            EsValorizado = x.EsValorizado,
            Total = x.Total,
            Saldo = x.Saldo,
            Estado = x.Estado,
            Cae = x.Cae
        }).ToList();

        return new PagedResult<ComprobanteListDto>(items, page, pageSize, totalCount);
    }
}
