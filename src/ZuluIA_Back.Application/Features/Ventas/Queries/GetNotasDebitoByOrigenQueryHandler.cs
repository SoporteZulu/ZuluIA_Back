using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Comprobantes.DTOs;
using ZuluIA_Back.Application.Features.Ventas.Common;

namespace ZuluIA_Back.Application.Features.Ventas.Queries;

public class GetNotasDebitoByOrigenQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetNotasDebitoByOrigenQuery, IReadOnlyList<ComprobanteListDto>>
{
    public async Task<IReadOnlyList<ComprobanteListDto>> Handle(GetNotasDebitoByOrigenQuery request, CancellationToken ct)
    {
        var query = db.Comprobantes
            .AsNoTracking()
            .Where(x => x.ComprobanteOrigenId == request.ComprobanteOrigenId && x.MotivoDebitoId.HasValue)
            .Join(
                db.TiposComprobante.AsNoTracking().Where(NotaDebitoWorkflowRules.TipoComprobantePredicate()),
                comprobante => comprobante.TipoComprobanteId,
                tipo => tipo.Id,
                (comprobante, tipo) => new { comprobante, tipo });

        var items = await query
            .OrderByDescending(x => x.comprobante.Fecha)
            .ThenByDescending(x => x.comprobante.Id)
            .Select(x => new
            {
                x.comprobante.Id,
                x.comprobante.SucursalId,
                x.comprobante.TipoComprobanteId,
                x.tipo.Descripcion,
                x.comprobante.Numero,
                x.comprobante.Fecha,
                x.comprobante.FechaVencimiento,
                x.comprobante.TerceroId,
                x.comprobante.MonedaId,
                x.comprobante.Total,
                x.comprobante.Saldo,
                x.comprobante.Estado,
                x.comprobante.Cae,
                x.comprobante.MotivoDebitoId,
                x.comprobante.ComprobanteOrigenId
            })
            .ToListAsync(ct);

        var terceroIds = items.Select(x => x.TerceroId).Distinct().ToList();
        var monedaIds = items.Select(x => x.MonedaId).Distinct().ToList();
        var motivoIds = items.Where(x => x.MotivoDebitoId.HasValue).Select(x => x.MotivoDebitoId!.Value).Distinct().ToList();
        var origenIds = items.Where(x => x.ComprobanteOrigenId.HasValue).Select(x => x.ComprobanteOrigenId!.Value).Distinct().ToList();

        var terceros = await db.Terceros.AsNoTracking()
            .Where(x => terceroIds.Contains(x.Id))
            .Select(x => new { x.Id, x.RazonSocial, x.Legajo })
            .ToDictionaryAsync(x => x.Id, ct);

        var monedas = await db.Monedas.AsNoTracking()
            .Where(x => monedaIds.Contains(x.Id))
            .Select(x => new { x.Id, x.Simbolo })
            .ToDictionaryAsync(x => x.Id, ct);

        var motivos = await db.MotivosDebito.AsNoTracking()
            .Where(x => motivoIds.Contains(x.Id))
            .Select(x => new { x.Id, x.Descripcion })
            .ToDictionaryAsync(x => x.Id, ct);

        var origenes = await db.Comprobantes.AsNoTracking()
            .Where(x => origenIds.Contains(x.Id))
            .Select(x => new { x.Id, Numero = x.Numero.Formateado, x.Fecha })
            .ToDictionaryAsync(x => x.Id, ct);

        return items.Select(x => new ComprobanteListDto
        {
            Id = x.Id,
            SucursalId = x.SucursalId,
            TipoComprobanteId = x.TipoComprobanteId,
            TipoComprobanteDescripcion = x.Descripcion,
            Prefijo = x.Numero.Prefijo,
            Numero = x.Numero.Numero,
            NumeroFormateado = x.Numero.Formateado,
            Fecha = x.Fecha,
            FechaVencimiento = x.FechaVencimiento,
            TerceroId = x.TerceroId,
            TerceroRazonSocial = terceros.GetValueOrDefault(x.TerceroId)?.RazonSocial ?? "—",
            TerceroLegajo = terceros.GetValueOrDefault(x.TerceroId)?.Legajo,
            MonedaId = x.MonedaId,
            MonedaSimbolo = monedas.GetValueOrDefault(x.MonedaId)?.Simbolo ?? "$",
            Total = x.Total,
            Saldo = x.Saldo,
            Estado = x.Estado.ToString().ToUpperInvariant(),
            Cae = x.Cae,
            MotivoDebitoId = x.MotivoDebitoId,
            MotivoDebitoDescripcion = x.MotivoDebitoId.HasValue
                ? motivos.GetValueOrDefault(x.MotivoDebitoId.Value)?.Descripcion
                : null,
            ComprobanteOrigenId = x.ComprobanteOrigenId,
            ComprobanteOrigenNumero = x.ComprobanteOrigenId.HasValue
                ? origenes.GetValueOrDefault(x.ComprobanteOrigenId.Value)?.Numero
                : null,
            ComprobanteOrigenFecha = x.ComprobanteOrigenId.HasValue
                ? origenes.GetValueOrDefault(x.ComprobanteOrigenId.Value)?.Fecha
                : null
        }).ToList().AsReadOnly();
    }
}
