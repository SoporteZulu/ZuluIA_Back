using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Franquicias;

namespace ZuluIA_Back.Application.Features.PlanTrabajo.Commands;

// ── Crear Plan de Trabajo ──────────────────────────────────────────────────

public record CrearPlanTrabajoCommand(
    string Nombre,
    long SucursalId,
    int Periodo,
    DateOnly FechaInicio,
    DateOnly FechaFin,
    string? Descripcion,
    long? UserId)
    : IRequest<Result<long>>;

public class CrearPlanTrabajoCommandHandler(IApplicationDbContext db)
    : IRequestHandler<CrearPlanTrabajoCommand, Result<long>>
{
    public async Task<Result<long>> Handle(CrearPlanTrabajoCommand request, CancellationToken ct)
    {
        var plan = Domain.Entities.Franquicias.PlanTrabajo.Crear(
            request.Nombre, request.SucursalId, request.Periodo,
            request.FechaInicio, request.FechaFin, request.Descripcion, request.UserId);
        db.PlanesTrabajo.Add(plan);
        await db.SaveChangesAsync(ct);
        return Result.Success(plan.Id);
    }
}

public class CrearPlanTrabajoCommandValidator : AbstractValidator<CrearPlanTrabajoCommand>
{
    public CrearPlanTrabajoCommandValidator()
    {
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(200);
        RuleFor(x => x.SucursalId).GreaterThan(0);
        RuleFor(x => x.Periodo).InclusiveBetween(190001, 209912);
        RuleFor(x => x.FechaFin).GreaterThanOrEqualTo(x => x.FechaInicio)
            .WithMessage("La fecha de fin debe ser posterior a la fecha de inicio.");
    }
}

// ── Agregar KPI al Plan ────────────────────────────────────────────────────

public record AgregarKpiCommand(
    long PlanTrabajoId,
    long? AspectoId,
    long? VariableId,
    string Descripcion,
    decimal ValorObjetivo,
    decimal Peso,
    long? UserId)
    : IRequest<Result<long>>;

public class AgregarKpiCommandHandler(IApplicationDbContext db)
    : IRequestHandler<AgregarKpiCommand, Result<long>>
{
    public async Task<Result<long>> Handle(AgregarKpiCommand request, CancellationToken ct)
    {
        var plan = await db.PlanesTrabajo
            .Include(p => p.Kpis)
            .FirstOrDefaultAsync(p => p.Id == request.PlanTrabajoId, ct);
        if (plan is null) return Result.Failure<long>("Plan de trabajo no encontrado.");

        try
        {
            var kpi = PlanTrabajoKpi.Crear(
                request.PlanTrabajoId, request.AspectoId, request.VariableId,
                request.Descripcion, request.ValorObjetivo, request.Peso);
            plan.AgregarKpi(kpi);
            await db.SaveChangesAsync(ct);
            return Result.Success(kpi.Id);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure<long>(ex.Message);
        }
    }
}

public class AgregarKpiCommandValidator : AbstractValidator<AgregarKpiCommand>
{
    public AgregarKpiCommandValidator()
    {
        RuleFor(x => x.PlanTrabajoId).GreaterThan(0);
        RuleFor(x => x.Descripcion).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ValorObjetivo).GreaterThan(0);
        RuleFor(x => x.Peso).GreaterThan(0);
    }
}

// ── Registrar Evaluación ───────────────────────────────────────────────────

public record DetalleEvaluacionInput(long KpiId, decimal ValorReal, decimal Puntaje, string? Observacion);

public record RegistrarEvaluacionCommand(
    long PlanTrabajoId,
    long SucursalId,
    int Periodo,
    DateOnly FechaEvaluacion,
    string? Observacion,
    List<DetalleEvaluacionInput> Detalles,
    long? UserId)
    : IRequest<Result<long>>;

public class RegistrarEvaluacionCommandHandler(IApplicationDbContext db)
    : IRequestHandler<RegistrarEvaluacionCommand, Result<long>>
{
    public async Task<Result<long>> Handle(RegistrarEvaluacionCommand request, CancellationToken ct)
    {
        var evaluacion = EvaluacionFranquicia.Crear(
            request.PlanTrabajoId, request.SucursalId, request.Periodo,
            request.FechaEvaluacion, request.Observacion, request.UserId);

        foreach (var d in request.Detalles)
        {
            var detalle = EvaluacionFranquiciaDetalle.Crear(
                0, d.KpiId, d.ValorReal, d.Puntaje, d.Observacion);
            evaluacion.AgregarDetalle(detalle);
        }

        db.EvaluacionesFranquicias.Add(evaluacion);
        await db.SaveChangesAsync(ct);
        return Result.Success(evaluacion.Id);
    }
}

// ── Cerrar Plan ────────────────────────────────────────────────────────────

public record CerrarPlanTrabajoCommand(long Id, long? UserId) : IRequest<Result>;

public class CerrarPlanTrabajoCommandHandler(IApplicationDbContext db)
    : IRequestHandler<CerrarPlanTrabajoCommand, Result>
{
    public async Task<Result> Handle(CerrarPlanTrabajoCommand request, CancellationToken ct)
    {
        var plan = await db.PlanesTrabajo.FindAsync([request.Id], ct);
        if (plan is null) return Result.Failure("Plan de trabajo no encontrado.");

        try
        {
            plan.Cerrar(request.UserId);
            await db.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(ex.Message);
        }
    }
}

public class CerrarPlanTrabajoCommandValidator : AbstractValidator<CerrarPlanTrabajoCommand>
{
    public CerrarPlanTrabajoCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

// ── Anular Plan ────────────────────────────────────────────────────────────

public record AnularPlanTrabajoCommand(long Id, long? UserId) : IRequest<Result>;

public class AnularPlanTrabajoCommandHandler(IApplicationDbContext db)
    : IRequestHandler<AnularPlanTrabajoCommand, Result>
{
    public async Task<Result> Handle(AnularPlanTrabajoCommand request, CancellationToken ct)
    {
        var plan = await db.PlanesTrabajo.FindAsync([request.Id], ct);
        if (plan is null) return Result.Failure("Plan de trabajo no encontrado.");

        try
        {
            plan.Anular(request.UserId);
            await db.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(ex.Message);
        }
    }
}

public class AnularPlanTrabajoCommandValidator : AbstractValidator<AnularPlanTrabajoCommand>
{
    public AnularPlanTrabajoCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

public class RegistrarEvaluacionCommandValidator : AbstractValidator<RegistrarEvaluacionCommand>
{
    public RegistrarEvaluacionCommandValidator()
    {
        RuleFor(x => x.PlanTrabajoId).GreaterThan(0);
        RuleFor(x => x.SucursalId).GreaterThan(0);
        RuleFor(x => x.Periodo).InclusiveBetween(190001, 209912);
        RuleFor(x => x.FechaEvaluacion).NotEmpty();
        RuleFor(x => x.Detalles).NotEmpty();
    }
}
