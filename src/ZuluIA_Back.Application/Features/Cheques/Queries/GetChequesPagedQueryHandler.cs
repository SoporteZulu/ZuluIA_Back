using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Cheques.DTOs;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Cheques.Queries;

public class GetChequesPagedQueryHandler(
    IChequeRepository repo,
    IMapper mapper,
    IApplicationDbContext db)
    : IRequestHandler<GetChequesPagedQuery, PagedResult<ChequeDto>>
{
    public async Task<PagedResult<ChequeDto>> Handle(
        GetChequesPagedQuery request,
        CancellationToken ct)
    {
        var result = await repo.GetPagedAsync(
            request.Page,
            request.PageSize,
            request.CajaId,
            request.TerceroId,
            request.Estado,
            request.Tipo,
            request.EsALaOrden,
            request.EsCruzado,
            request.Banco,
            request.NroCheque,
            request.Titular,
            request.Desde,
            request.Hasta,
            ct);

        var items = mapper.Map<List<ChequeDto>>(result.Items);

        var cajaIds = result.Items.Select(x => x.CajaId).Distinct().ToList();
        var terceroIds = result.Items.Where(x => x.TerceroId.HasValue).Select(x => x.TerceroId!.Value).Distinct().ToList();
        var monedaIds = result.Items.Select(x => x.MonedaId).Distinct().ToList();
        var chequeraIds = result.Items.Where(x => x.ChequeraId.HasValue).Select(x => x.ChequeraId!.Value).Distinct().ToList();
        var comprobanteOrigenIds = result.Items.Where(x => x.ComprobanteOrigenId.HasValue).Select(x => x.ComprobanteOrigenId!.Value).Distinct().ToList();

        var cajas = await db.CajasCuentasBancarias.AsNoTracking()
            .Where(x => cajaIds.Contains(x.Id))
            .Select(x => new { x.Id, x.Descripcion })
            .ToDictionaryAsync(x => x.Id, ct);

        var terceros = await db.Terceros.AsNoTracking()
            .Where(x => terceroIds.Contains(x.Id))
            .Select(x => new { x.Id, x.RazonSocial })
            .ToDictionaryAsync(x => x.Id, ct);

        var monedas = await db.Monedas.AsNoTracking()
            .Where(x => monedaIds.Contains(x.Id))
            .Select(x => new { x.Id, x.Simbolo })
            .ToDictionaryAsync(x => x.Id, ct);

        var chequeras = await db.Chequeras.AsNoTracking()
            .Where(x => chequeraIds.Contains(x.Id))
            .Select(x => new { x.Id, Descripcion = x.Banco + " - " + x.NroCuenta })
            .ToDictionaryAsync(x => x.Id, ct);

        var comprobantes = await db.Comprobantes.AsNoTracking()
            .Where(x => comprobanteOrigenIds.Contains(x.Id))
            .Select(x => new { x.Id, Numero = x.Numero.Formateado })
            .ToDictionaryAsync(x => x.Id, ct);

        foreach (var item in items)
        {
            item.CajaDescripcion = cajas.GetValueOrDefault(item.CajaId)?.Descripcion;
            item.TerceroRazonSocial = item.TerceroId.HasValue
                ? terceros.GetValueOrDefault(item.TerceroId.Value)?.RazonSocial
                : null;
            item.MonedaSimbolo = monedas.GetValueOrDefault(item.MonedaId)?.Simbolo;
            item.ChequeraDescripcion = item.ChequeraId.HasValue
                ? chequeras.GetValueOrDefault(item.ChequeraId.Value)?.Descripcion
                : null;
            item.ComprobanteOrigenNumero = item.ComprobanteOrigenId.HasValue
                ? comprobantes.GetValueOrDefault(item.ComprobanteOrigenId.Value)?.Numero
                : null;
        }

        return new PagedResult<ChequeDto>(items.AsReadOnly(), result.Page, result.PageSize, result.TotalCount);
    }
}