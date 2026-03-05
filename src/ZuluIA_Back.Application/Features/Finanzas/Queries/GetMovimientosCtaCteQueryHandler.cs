using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Finanzas.DTOs;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;
using ZuluIA_Back.Infrastructure.Common.Interfaces;

namespace ZuluIA_Back.Application.Features.Finanzas.Queries;

public class GetMovimientosCtaCteQueryHandler(
    IMovimientoCtaCteRepository repo,
    IApplicationDbContext db)
    : IRequestHandler<GetMovimientosCtaCteQuery, PagedResult<MovimientoCtaCteDto>>
{
    public async Task<PagedResult<MovimientoCtaCteDto>> Handle(
        GetMovimientosCtaCteQuery request,
        CancellationToken ct)
    {
        var result = await repo.GetPagedAsync(
            request.Page, request.PageSize,
            request.TerceroId, request.SucursalId,
            request.MonedaId, request.Desde, request.Hasta, ct);

        var monedaIds = result.Items.Select(x => x.MonedaId).Distinct().ToList();
        var comprobanteIds = result.Items
            .Where(x => x.ComprobanteId.HasValue)
            .Select(x => x.ComprobanteId!.Value).Distinct().ToList();

        var monedas = await db.Monedas.AsNoTracking()
            .Where(x => monedaIds.Contains(x.Id))
            .Select(x => new { x.Id, x.Simbolo })
            .ToDictionaryAsync(x => x.Id, ct);

        var numeros = await db.Comprobantes.AsNoTracking()
            .Where(x => comprobanteIds.Contains(x.Id))
            .Select(x => new { x.Id, x.Numero.Prefijo, x.Numero.Numero })
            .ToDictionaryAsync(x => x.Id, ct);

        var dtos = result.Items.Select(m => new MovimientoCtaCteDto
        {
            Id                = m.Id,
            TerceroId         = m.TerceroId,
            SucursalId        = m.SucursalId,
            MonedaId          = m.MonedaId,
            MonedaSimbolo     = monedas.GetValueOrDefault(m.MonedaId)?.Simbolo ?? "$",
            ComprobanteId     = m.ComprobanteId,
            NumeroComprobante = m.ComprobanteId.HasValue && numeros.ContainsKey(m.ComprobanteId.Value)
                ? $"{numeros[m.ComprobanteId.Value].Prefijo:D4}-{numeros[m.ComprobanteId.Value].Numero:D8}"
                : null,
            Fecha             = m.Fecha,
            Debe              = m.Debe,
            Haber             = m.Haber,
            Saldo             = m.Saldo,
            Descripcion       = m.Descripcion,
            CreatedAt         = m.CreatedAt
        }).ToList();

        return new PagedResult<MovimientoCtaCteDto>(
            dtos, result.Page, result.PageSize, result.TotalCount);
    }
}