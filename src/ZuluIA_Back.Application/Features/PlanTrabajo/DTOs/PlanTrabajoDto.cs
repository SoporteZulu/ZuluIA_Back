namespace ZuluIA_Back.Application.Features.PlanTrabajo.DTOs;

public record PlanTrabajoListDto(
    long Id,
    string Nombre,
    long SucursalId,
    int Periodo,
    DateOnly FechaInicio,
    DateOnly FechaFin,
    string Estado);

public record PlanTrabajoDto(
    long Id,
    string Nombre,
    long SucursalId,
    int Periodo,
    DateOnly FechaInicio,
    DateOnly FechaFin,
    string Estado,
    string? Descripcion,
    IReadOnlyCollection<PlanTrabajoKpiDto> Kpis);

public record PlanTrabajoKpiDto(
    long Id,
    long? AspectoId,
    long? VariableId,
    string Descripcion,
    decimal ValorObjetivo,
    decimal Peso);

public record EvaluacionFranquiciaDto(
    long Id,
    long PlanTrabajoId,
    long SucursalId,
    int Periodo,
    decimal PuntajeTotal,
    DateOnly FechaEvaluacion,
    string? Observacion,
    IReadOnlyCollection<EvaluacionDetalleDto> Detalles);

public record EvaluacionDetalleDto(
    long Id,
    long KpiId,
    decimal ValorReal,
    decimal Puntaje,
    string? Observacion);
