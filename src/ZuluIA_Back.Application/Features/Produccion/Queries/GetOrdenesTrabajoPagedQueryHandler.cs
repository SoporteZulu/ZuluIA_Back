using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Produccion.DTOs;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Produccion.Queries;

public class GetOrdenesTrabajoPagedQueryHandler(
    IOrdenTrabajoRepository repo,
    IApplicationDbContext db)
    : IRequestHandler<GetOrdenesTrabajoPagedQuery, PagedResult<OrdenTrabajoDto>>
{
    public async Task<PagedResult<OrdenTrabajoDto>> Handle(
        GetOrdenesTrabajoPagedQuery request,
        CancellationToken ct)
    {
        var result = await repo.GetPagedAsync(
            request.Page, request.PageSize,
            request.SucursalId, request.FormulaId,
            request.Estado, request.Desde, request.Hasta, ct);

        var formulaIds = result.Items.Select(x => x.FormulaId).Distinct().ToList();
        var depositoIds = result.Items
            .SelectMany(x => new[] { x.DepositoOrigenId, x.DepositoDestinoId })
            .Distinct().ToList();

        var formulas = await db.FormulasProduccion.AsNoTracking()
            .Where(x => formulaIds.Contains(x.Id))
            .Select(x => new { x.Id, x.Codigo, x.Descripcion })
            .ToDictionaryAsync(x => x.Id, ct);

        var depositos = await db.Depositos.AsNoTracking()
            .Where(x => depositoIds.Contains(x.Id))
            .Select(x => new { x.Id, x.Descripcion })
            .ToDictionaryAsync(x => x.Id, ct);

        var dtos = result.Items.Select(ot => new OrdenTrabajoDto
        {
            Id                        = ot.Id,
            SucursalId                = ot.SucursalId,
            FormulaId                 = ot.FormulaId,
            FormulaCodigo             = formulas.GetValueOrDefault(ot.FormulaId)?.Codigo      ?? "—",
            FormulaDescripcion        = formulas.GetValueOrDefault(ot.FormulaId)?.Descripcion ?? "—",
            DepositoOrigenId          = ot.DepositoOrigenId,
            DepositoOrigenDescripcion = depositos.GetValueOrDefault(ot.DepositoOrigenId)?.Descripcion  ?? "—",
            DepositoDestinoId         = ot.DepositoDestinoId,
            DepositoDestinoDescripcion = depositos.GetValueOrDefault(ot.DepositoDestinoId)?.Descripcion ?? "—",
            Fecha                     = ot.Fecha,
            FechaFinPrevista          = ot.FechaFinPrevista,
            FechaFinReal              = ot.FechaFinReal,
            Cantidad                  = ot.Cantidad,
            CantidadProducida         = ot.CantidadProducida,
            Estado                    = ot.Estado.ToString().ToUpperInvariant(),
            Observacion               = ot.Observacion,
            CreatedAt                 = ot.CreatedAt
        }).ToList();

        return new PagedResult<OrdenTrabajoDto>(
            dtos, result.Page, result.PageSize, result.TotalCount);
    }
}