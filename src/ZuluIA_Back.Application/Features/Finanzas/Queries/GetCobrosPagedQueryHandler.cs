using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Finanzas.DTOs;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;
using ZuluIA_Back.Infrastructure.Common.Interfaces;

namespace ZuluIA_Back.Application.Features.Finanzas.Queries;

public class GetCobrosPagedQueryHandler(
    ICobroRepository repo,
    IApplicationDbContext db)
    : IRequestHandler<GetCobrosPagedQuery, PagedResult<CobroListDto>>
{
    public async Task<PagedResult<CobroListDto>> Handle(
        GetCobrosPagedQuery request,
        CancellationToken ct)
    {
        var result = await repo.GetPagedAsync(
            request.Page, request.PageSize,
            request.SucursalId, request.TerceroId,
            request.Desde, request.Hasta, ct);

        var terceroIds = result.Items.Select(x => x.TerceroId).Distinct().ToList();
        var monedaIds = result.Items.Select(x => x.MonedaId).Distinct().ToList();

        var terceros = await db.Terceros.AsNoTracking()
            .Where(x => terceroIds.Contains(x.Id))
            .Select(x => new { x.Id, x.RazonSocial })
            .ToDictionaryAsync(x => x.Id, ct);

        var monedas = await db.Monedas.AsNoTracking()
            .Where(x => monedaIds.Contains(x.Id))
            .Select(x => new { x.Id, x.Simbolo })
            .ToDictionaryAsync(x => x.Id, ct);

        var dtos = result.Items.Select(c => new CobroListDto
        {
            Id                 = c.Id,
            TerceroId          = c.TerceroId,
            TerceroRazonSocial = terceros.GetValueOrDefault(c.TerceroId)?.RazonSocial ?? "—",
            Fecha              = c.Fecha,
            MonedaSimbolo      = monedas.GetValueOrDefault(c.MonedaId)?.Simbolo ?? "$",
            Total              = c.Total,
            Estado             = c.Estado.ToString().ToUpperInvariant(),
            NroCierre          = c.NroCierre
        }).ToList();

        return new PagedResult<CobroListDto>(
            dtos, result.Page, result.PageSize, result.TotalCount);
    }
}