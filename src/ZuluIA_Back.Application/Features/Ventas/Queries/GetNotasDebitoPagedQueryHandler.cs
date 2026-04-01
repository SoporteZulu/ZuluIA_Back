using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Comprobantes.DTOs;
using ZuluIA_Back.Application.Features.Ventas.Common;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Ventas.Queries;

public class GetNotasDebitoPagedQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetNotasDebitoPagedQuery, PagedResult<ComprobanteListDto>>
{
    public async Task<PagedResult<ComprobanteListDto>> Handle(GetNotasDebitoPagedQuery request, CancellationToken ct)
    {
        var tipoIds = await db.TiposComprobante
            .AsNoTracking()
            .Where(NotaDebitoWorkflowRules.TipoComprobantePredicate())
            .Select(x => x.Id)
            .ToListAsync(ct);

        if (tipoIds.Count == 0)
            return new PagedResult<ComprobanteListDto>([], request.Page, request.PageSize, 0);

        var query = db.Comprobantes
            .AsNoTracking()
            .Where(x => tipoIds.Contains(x.TipoComprobanteId));

        if (request.SucursalId.HasValue)
            query = query.Where(x => x.SucursalId == request.SucursalId.Value);

        if (request.TerceroId.HasValue)
            query = query.Where(x => x.TerceroId == request.TerceroId.Value);

        if (request.Estado.HasValue)
            query = query.Where(x => x.Estado == request.Estado.Value);

        if (request.Desde.HasValue)
            query = query.Where(x => x.Fecha >= request.Desde.Value);

        if (request.Hasta.HasValue)
            query = query.Where(x => x.Fecha <= request.Hasta.Value);

        if (request.MotivoDebitoId.HasValue)
            query = query.Where(x => x.MotivoDebitoId == request.MotivoDebitoId.Value);

        if (request.ComprobanteOrigenId.HasValue)
            query = query.Where(x => x.ComprobanteOrigenId == request.ComprobanteOrigenId.Value);

        var totalCount = await query.CountAsync(ct);
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;

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
                x.MotivoDebitoId,
                x.ComprobanteOrigenId
            })
            .ToListAsync(ct);

        var terceroIds = rows.Select(x => x.TerceroId).Distinct().ToList();
        var tipoComprobanteIds = rows.Select(x => x.TipoComprobanteId).Distinct().ToList();
        var monedaIds = rows.Select(x => x.MonedaId).Distinct().ToList();
        var motivoIds = rows.Where(x => x.MotivoDebitoId.HasValue)
            .Select(x => x.MotivoDebitoId!.Value)
            .Distinct()
            .ToList();
        var comprobantesOrigenIds = rows.Where(x => x.ComprobanteOrigenId.HasValue)
            .Select(x => x.ComprobanteOrigenId!.Value)
            .Distinct()
            .ToList();

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

        var motivosDebito = motivoIds.Count > 0
            ? await db.MotivosDebito
                .AsNoTracking()
                .Where(x => motivoIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.Descripcion, ct)
            : new Dictionary<long, string>();

        var comprobantesOrigen = comprobantesOrigenIds.Count > 0
            ? await db.Comprobantes
                .AsNoTracking()
                .Where(x => comprobantesOrigenIds.Contains(x.Id))
                .ToDictionaryAsync(
                    x => x.Id,
                    x => new NroComprobanteOrigenLookup(x.Numero.Prefijo, x.Numero.Numero, x.Fecha),
                    ct)
            : new Dictionary<long, NroComprobanteOrigenLookup>();

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
            Total = x.Total,
            Saldo = x.Saldo,
            MotivoDebitoId = x.MotivoDebitoId,
            MotivoDebitoDescripcion = x.MotivoDebitoId.HasValue
                ? motivosDebito.GetValueOrDefault(x.MotivoDebitoId.Value)
                : null,
            ComprobanteOrigenId = x.ComprobanteOrigenId,
            ComprobanteOrigenNumero = x.ComprobanteOrigenId.HasValue
                ? FormatearNumeroComprobanteOrigen(comprobantesOrigen.GetValueOrDefault(x.ComprobanteOrigenId.Value))
                : null,
            ComprobanteOrigenFecha = x.ComprobanteOrigenId.HasValue
                ? comprobantesOrigen.GetValueOrDefault(x.ComprobanteOrigenId.Value)?.Fecha
                : null,
            Estado = x.Estado,
            Cae = x.Cae
        }).ToList();

        return new PagedResult<ComprobanteListDto>(items, page, pageSize, totalCount);
    }

    private static string? FormatearNumeroComprobanteOrigen(NroComprobanteOrigenLookup? origen)
        => origen is null ? null : $"{origen.Prefijo:D4}-{origen.Numero:D8}";

    private sealed record NroComprobanteOrigenLookup(short Prefijo, long Numero, DateOnly Fecha);
}
