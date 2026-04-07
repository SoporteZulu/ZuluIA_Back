using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Cheques.DTOs;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Enums;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Cheques.Queries;

public class GetChequesPropiosPagedQueryHandler(
    IChequeRepository repo,
    IMapper mapper,
    IApplicationDbContext db)
    : IRequestHandler<GetChequesPropiosPagedQuery, PagedResult<ChequePropio>>
{
    public async Task<PagedResult<ChequePropio>> Handle(
        GetChequesPropiosPagedQuery request,
        CancellationToken ct)
    {
        var result = await repo.GetPagedAsync(
            request.Page,
            request.PageSize,
            request.CajaId,
            null,
            request.Estado,
            TipoCheque.Propio,
            null,
            null,
            null,
            request.NroCheque,
            null,
            request.Desde,
            request.Hasta,
            ct);

        var items = mapper.Map<List<ChequePropio>>(result.Items);

        var cajaIds = result.Items.Select(x => x.CajaId).Distinct().ToList();
        var monedaIds = result.Items.Select(x => x.MonedaId).Distinct().ToList();
        var chequeraIds = result.Items.Where(x => x.ChequeraId.HasValue).Select(x => x.ChequeraId!.Value).Distinct().ToList();

        var cajas = await db.CajasCuentasBancarias.AsNoTracking()
            .Where(x => cajaIds.Contains(x.Id))
            .Select(x => new { x.Id, x.Descripcion })
            .ToDictionaryAsync(x => x.Id, ct);

        var monedas = await db.Monedas.AsNoTracking()
            .Where(x => monedaIds.Contains(x.Id))
            .Select(x => new { x.Id, x.Simbolo })
            .ToDictionaryAsync(x => x.Id, ct);

        var chequeras = await db.Chequeras.AsNoTracking()
            .Where(x => chequeraIds.Contains(x.Id))
            .Select(x => new { x.Id, x.Banco, x.NroCuenta })
            .ToDictionaryAsync(x => x.Id, ct);

        foreach (var item in items)
        {
            item.CajaDescripcion = cajas.GetValueOrDefault(item.CajaId)?.Descripcion;
            item.MonedaSimbolo = monedas.GetValueOrDefault(item.MonedaId)?.Simbolo;

            if (item.ChequeraId.HasValue && chequeras.TryGetValue(item.ChequeraId.Value, out var chequera))
            {
                item.BancoDescripcion = chequera.Banco;
                item.NroCuenta = chequera.NroCuenta;
                item.ChequeraDescripcion = $"{chequera.Banco} - {chequera.NroCuenta}";
            }
        }

        return new PagedResult<ChequePropio>(items.AsReadOnly(), result.Page, result.PageSize, result.TotalCount);
    }
}
