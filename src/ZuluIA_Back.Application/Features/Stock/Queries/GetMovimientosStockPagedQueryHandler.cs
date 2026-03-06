using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Stock.DTOs;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Stock.Queries;

public class GetMovimientosStockPagedQueryHandler(
    IMovimientoStockRepository repo,
    IApplicationDbContext db)
    : IRequestHandler<GetMovimientosStockPagedQuery, PagedResult<MovimientoStockDto>>
{
    public async Task<PagedResult<MovimientoStockDto>> Handle(
        GetMovimientosStockPagedQuery request,
        CancellationToken ct)
    {
        var result = await repo.GetPagedAsync(
            request.Page, request.PageSize,
            request.ItemId, request.DepositoId,
            request.Tipo, request.Desde, request.Hasta,
            ct);

        // Enriquecer con descripciones
        var itemIds = result.Items.Select(x => x.ItemId).Distinct().ToList();
        var depositoIds = result.Items.Select(x => x.DepositoId).Distinct().ToList();

        var items = await db.Items
            .AsNoTracking()
            .Where(x => itemIds.Contains(x.Id))
            .Select(x => new { x.Id, x.Codigo, x.Descripcion })
            .ToDictionaryAsync(x => x.Id, ct);

        var depositos = await db.Depositos
            .AsNoTracking()
            .Where(x => depositoIds.Contains(x.Id))
            .Select(x => new { x.Id, x.Descripcion })
            .ToDictionaryAsync(x => x.Id, ct);

        var dtos = result.Items.Select(m => new MovimientoStockDto
        {
            Id                  = m.Id,
            ItemId              = m.ItemId,
            ItemCodigo          = items.GetValueOrDefault(m.ItemId)?.Codigo     ?? "—",
            ItemDescripcion     = items.GetValueOrDefault(m.ItemId)?.Descripcion ?? "—",
            DepositoId          = m.DepositoId,
            DepositoDescripcion = depositos.GetValueOrDefault(m.DepositoId)?.Descripcion ?? "—",
            Fecha               = m.Fecha,
            TipoMovimiento      = m.TipoMovimiento.ToString(),
            Cantidad            = m.Cantidad,
            SaldoResultante     = m.SaldoResultante,
            OrigenTabla         = m.OrigenTabla,
            OrigenId            = m.OrigenId,
            Observacion         = m.Observacion,
            CreatedAt           = m.CreatedAt,
            CreatedBy           = m.CreatedBy
        }).ToList();

        return new PagedResult<MovimientoStockDto>(
            dtos, result.Page, result.PageSize, result.TotalCount);
    }
}