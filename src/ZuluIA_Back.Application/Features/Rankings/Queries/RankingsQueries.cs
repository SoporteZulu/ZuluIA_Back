using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;

namespace ZuluIA_Back.Application.Features.Rankings.Queries;

// ── DTOs ──────────────────────────────────────────────────────────────────

public record RankingClienteDto(long TerceroId, decimal TotalFacturado, int CantidadOperaciones);
public record RankingItemDto(long ItemId, decimal TotalVendido, decimal CantidadVendida);
public record AnalisisMensualDto(int Anio, int Mes, decimal TotalVentas, int CantidadComprobantes);

// ── Ranking clientes ──────────────────────────────────────────────────────

public record GetRankingClientesQuery(
    long SucursalId,
    DateOnly Desde,
    DateOnly Hasta,
    int Top = 10)
    : IRequest<List<RankingClienteDto>>;

public class GetRankingClientesQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetRankingClientesQuery, List<RankingClienteDto>>
{
    public async Task<List<RankingClienteDto>> Handle(GetRankingClientesQuery request, CancellationToken ct)
    {
        return await db.Comprobantes.AsNoTracking()
            .Where(c => c.SucursalId == request.SucursalId
                     && c.Fecha >= request.Desde && c.Fecha <= request.Hasta
                     && c.DeletedAt == null)
            .GroupBy(c => c.TerceroId)
            .Select(g => new RankingClienteDto(g.Key, g.Sum(c => c.Total), g.Count()))
            .OrderByDescending(r => r.TotalFacturado)
            .Take(request.Top)
            .ToListAsync(ct);
    }
}

// ── Ranking ítems ─────────────────────────────────────────────────────────

public record GetRankingItemsQuery(
    long SucursalId,
    DateOnly Desde,
    DateOnly Hasta,
    int Top = 10)
    : IRequest<List<RankingItemDto>>;

public class GetRankingItemsQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetRankingItemsQuery, List<RankingItemDto>>
{
    public async Task<List<RankingItemDto>> Handle(GetRankingItemsQuery request, CancellationToken ct)
    {
        var compIds = await db.Comprobantes.AsNoTracking()
            .Where(c => c.SucursalId == request.SucursalId
                     && c.Fecha >= request.Desde && c.Fecha <= request.Hasta
                     && c.DeletedAt == null)
            .Select(c => c.Id)
            .ToListAsync(ct);

        return await db.ComprobantesItems.AsNoTracking()
            .Where(ci => compIds.Contains(ci.ComprobanteId))
            .GroupBy(ci => ci.ItemId)
            .Select(g => new RankingItemDto(
                g.Key,
                g.Sum(ci => ci.TotalLinea),
                g.Sum(ci => ci.Cantidad)))
            .OrderByDescending(r => r.TotalVendido)
            .Take(request.Top)
            .ToListAsync(ct);
    }
}

// ── Análisis mensual ──────────────────────────────────────────────────────

public record GetAnalisisMensualQuery(long SucursalId, int Anio) : IRequest<List<AnalisisMensualDto>>;

public class GetAnalisisMensualQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetAnalisisMensualQuery, List<AnalisisMensualDto>>
{
    public async Task<List<AnalisisMensualDto>> Handle(GetAnalisisMensualQuery request, CancellationToken ct)
    {
        var desde = new DateOnly(request.Anio, 1, 1);
        var hasta = new DateOnly(request.Anio, 12, 31);

        return await db.Comprobantes.AsNoTracking()
            .Where(c => c.SucursalId == request.SucursalId
                     && c.Fecha >= desde && c.Fecha <= hasta
                     && c.DeletedAt == null)
            .GroupBy(c => new { c.Fecha.Year, c.Fecha.Month })
            .Select(g => new AnalisisMensualDto(g.Key.Year, g.Key.Month, g.Sum(c => c.Total), g.Count()))
            .OrderBy(r => r.Anio).ThenBy(r => r.Mes)
            .ToListAsync(ct);
    }
}
