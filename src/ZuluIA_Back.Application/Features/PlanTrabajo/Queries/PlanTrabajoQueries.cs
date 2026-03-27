using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Application.Features.PlanTrabajo.DTOs;

namespace ZuluIA_Back.Application.Features.PlanTrabajo.Queries;

public record GetPlanesTrabajoPagedQuery(
    long SucursalId,
    int? Periodo = null,
    string? Estado = null,
    int Page = 1,
    int PageSize = 20)
    : IRequest<PagedResult<PlanTrabajoListDto>>;

public class GetPlanesTrabajoPagedQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetPlanesTrabajoPagedQuery, PagedResult<PlanTrabajoListDto>>
{
    public async Task<PagedResult<PlanTrabajoListDto>> Handle(GetPlanesTrabajoPagedQuery request, CancellationToken ct)
    {
        var query = db.PlanesTrabajo.AsNoTracking()
            .Where(p => p.SucursalId == request.SucursalId && p.DeletedAt == null);

        if (request.Periodo.HasValue) query = query.Where(p => p.Periodo == request.Periodo);
        if (!string.IsNullOrWhiteSpace(request.Estado))
            query = query.Where(p => p.Estado.ToString() == request.Estado);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(p => p.Periodo)
            .Skip((request.Page - 1) * request.PageSize).Take(request.PageSize)
            .Select(p => new PlanTrabajoListDto(p.Id, p.Nombre, p.SucursalId, p.Periodo,
                p.FechaInicio, p.FechaFin, p.Estado.ToString()))
            .ToListAsync(ct);

        return new PagedResult<PlanTrabajoListDto>(items, request.Page, request.PageSize, total);
    }
}

public record GetPlanTrabajoDetalleQuery(long Id) : IRequest<PlanTrabajoDto?>;

public class GetPlanTrabajoDetalleQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetPlanTrabajoDetalleQuery, PlanTrabajoDto?>
{
    public async Task<PlanTrabajoDto?> Handle(GetPlanTrabajoDetalleQuery request, CancellationToken ct)
    {
        var plan = await db.PlanesTrabajo.AsNoTracking()
            .Include(p => p.Kpis)
            .FirstOrDefaultAsync(p => p.Id == request.Id, ct);
        if (plan is null) return null;

        return new PlanTrabajoDto(
            plan.Id, plan.Nombre, plan.SucursalId, plan.Periodo,
            plan.FechaInicio, plan.FechaFin, plan.Estado.ToString(), plan.Descripcion,
            plan.Kpis.Select(k => new PlanTrabajoKpiDto(
                k.Id, k.AspectoId, k.VariableId, k.Descripcion, k.ValorObjetivo, k.Peso))
            .ToList());
    }
}

public record GetEvaluacionDetalleQuery(long Id) : IRequest<EvaluacionFranquiciaDto?>;

public class GetEvaluacionDetalleQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetEvaluacionDetalleQuery, EvaluacionFranquiciaDto?>
{
    public async Task<EvaluacionFranquiciaDto?> Handle(GetEvaluacionDetalleQuery request, CancellationToken ct)
    {
        var eval = await db.EvaluacionesFranquicias.AsNoTracking()
            .Include(e => e.Detalles)
            .FirstOrDefaultAsync(e => e.Id == request.Id, ct);
        if (eval is null) return null;

        return new EvaluacionFranquiciaDto(
            eval.Id, eval.PlanTrabajoId, eval.SucursalId, eval.Periodo,
            eval.PuntajeTotal, eval.FechaEvaluacion, eval.Observacion,
            eval.Detalles.Select(d => new EvaluacionDetalleDto(
                d.Id, d.KpiId, d.ValorReal, d.Puntaje, d.Observacion)).ToList());
    }
}
