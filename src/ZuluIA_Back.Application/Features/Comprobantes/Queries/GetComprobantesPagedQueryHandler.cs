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
        var comprobanteIds = result.Items.Select(x => x.Id).Distinct().ToList();
        var motivoIds = result.Items.Where(x => x.MotivoDebitoId.HasValue)
            .Select(x => x.MotivoDebitoId!.Value)
            .Distinct()
            .ToList();
        var origenIds = result.Items.Where(x => x.ComprobanteOrigenId.HasValue)
            .Select(x => x.ComprobanteOrigenId!.Value)
            .Distinct()
            .ToList();
        var depositoIds = result.Items.Where(x => x.DepositoOrigenId.HasValue)
            .Select(x => x.DepositoOrigenId!.Value)
            .Distinct()
            .ToList();

        var terceros = await db.Terceros
            .AsNoTracking()
            .Where(x => terceroIds.Contains(x.Id))
            .Select(x => new { x.Id, x.RazonSocial, x.Legajo })
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

        var cotPorComprobante = await db.ComprobantesCot
            .AsNoTracking()
            .Where(x => comprobanteIds.Contains(x.ComprobanteId))
            .Select(x => new { x.ComprobanteId, x.Numero, x.FechaVigencia })
            .ToDictionaryAsync(x => x.ComprobanteId, ct);

        var depositos = await db.Depositos
            .AsNoTracking()
            .Where(x => depositoIds.Contains(x.Id))
            .Select(x => new { x.Id, x.Descripcion })
            .ToDictionaryAsync(x => x.Id, ct);

        var motivos = await db.MotivosDebito
            .AsNoTracking()
            .Where(x => motivoIds.Contains(x.Id))
            .Select(x => new { x.Id, x.Descripcion })
            .ToDictionaryAsync(x => x.Id, ct);

        var origenes = await db.Comprobantes
            .AsNoTracking()
            .Where(x => origenIds.Contains(x.Id))
            .Select(x => new { x.Id, Numero = x.Numero.Formateado, x.Fecha })
            .ToDictionaryAsync(x => x.Id, ct);

        var dtos = result.Items.Select(c => new ComprobanteListDto
        {
            Id                          = c.Id,
            SucursalId                  = c.SucursalId,
            SucursalCodigo              = c.SucursalId.ToString(),
            TipoComprobanteId           = c.TipoComprobanteId,
            TipoComprobanteDescripcion  = tipos.GetValueOrDefault(c.TipoComprobanteId)?.Descripcion ?? "—",
            Prefijo                     = c.Numero.Prefijo,
            Numero                      = c.Numero.Numero,
            NumeroFormateado            = c.Numero.Formateado,
            Fecha                       = c.Fecha,
            FechaVencimiento            = c.FechaVencimiento,
            TerceroId                   = c.TerceroId,
            TerceroRazonSocial          = terceros.GetValueOrDefault(c.TerceroId)?.RazonSocial ?? "—",
            TerceroLegajo               = terceros.GetValueOrDefault(c.TerceroId)?.Legajo,
            MonedaId                    = c.MonedaId,
            MonedaSimbolo               = monedas.GetValueOrDefault(c.MonedaId)?.Simbolo ?? "$",
            DepositoOrigenId            = c.DepositoOrigenId,
            DepositoDescripcion         = c.DepositoOrigenId.HasValue
                ? depositos.GetValueOrDefault(c.DepositoOrigenId.Value)?.Descripcion
                : null,
            CotNumero                   = cotPorComprobante.GetValueOrDefault(c.Id)?.Numero,
            CotFechaVigencia            = cotPorComprobante.GetValueOrDefault(c.Id)?.FechaVigencia,
            EstadoLogistico             = c.EstadoLogistico,
            EsValorizado                = c.EsValorizado,
            Total                       = c.Total,
            Saldo                       = c.Saldo,
            MotivoDebitoId              = c.MotivoDebitoId,
            MotivoDebitoDescripcion     = c.MotivoDebitoId.HasValue
                ? motivos.GetValueOrDefault(c.MotivoDebitoId.Value)?.Descripcion
                : null,
            ComprobanteOrigenId         = c.ComprobanteOrigenId,
            ComprobanteOrigenNumero     = c.ComprobanteOrigenId.HasValue
                ? origenes.GetValueOrDefault(c.ComprobanteOrigenId.Value)?.Numero
                : null,
            ComprobanteOrigenFecha      = c.ComprobanteOrigenId.HasValue
                ? origenes.GetValueOrDefault(c.ComprobanteOrigenId.Value)?.Fecha
                : null,
            Estado                      = c.Estado,
            Cae                         = c.Cae
            //TieneCae                    = !string.IsNullOrEmpty(c.Cae)
        }).ToList();

        return new PagedResult<ComprobanteListDto>(
            dtos, result.Page, result.PageSize, result.TotalCount);
    }
}
