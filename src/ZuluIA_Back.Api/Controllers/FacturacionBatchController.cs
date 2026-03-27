using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Api.Security;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Facturacion.Commands;
using ZuluIA_Back.Application.Features.Integraciones.Services;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Api.Controllers;

[RequirePermission("FACTURACION.BATCH")]
[AuditCriticalOperation("FACTURACION_BATCH")]
public class FacturacionBatchController(IMediator mediator, IApplicationDbContext db, BatchSchedulerService schedulerService, OperacionesBatchSettingsService settingsService) : BaseController(mediator)
{
    [HttpPost("masiva")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> FacturarMasivo([FromBody] FacturarDocumentosMasivoCommand command, CancellationToken ct)
        => FromResult(await Mediator.Send(command, ct));

    [HttpPost("automatica")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> FacturarAutomatica([FromBody] EjecutarFacturacionAutomaticaCommand command, CancellationToken ct)
        => FromResult(await Mediator.Send(command, ct));

    [HttpGet("documentos-elegibles")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDocumentosElegibles(
        [FromQuery] long sucursalId,
        [FromQuery] long tipoComprobanteOrigenId,
        [FromQuery] long tipoComprobanteDestinoId,
        [FromQuery] DateOnly desde,
        [FromQuery] DateOnly hasta,
        [FromQuery] long? terceroId,
        [FromQuery] bool soloEmitidos = true,
        CancellationToken ct = default)
    {
        var query = db.Comprobantes.AsNoTracking()
            .Where(x => x.SucursalId == sucursalId
                && x.TipoComprobanteId == tipoComprobanteOrigenId
                && x.Fecha >= desde
                && x.Fecha <= hasta
                && x.Estado != EstadoComprobante.Anulado
                && x.Estado != EstadoComprobante.Convertido
                && !x.IsDeleted);

        if (terceroId.HasValue)
            query = query.Where(x => x.TerceroId == terceroId.Value);

        if (soloEmitidos)
            query = query.Where(x => x.Estado == EstadoComprobante.Emitido);

        var items = await query
            .Where(x => !db.Comprobantes.Any(h => h.ComprobanteOrigenId == x.Id && h.TipoComprobanteId == tipoComprobanteDestinoId && h.Estado != EstadoComprobante.Anulado && !h.IsDeleted))
            .OrderBy(x => x.Fecha)
            .ThenBy(x => x.Id)
            .Select(x => new
            {
                x.Id,
                Numero = x.Numero.Formateado,
                x.Fecha,
                x.TerceroId,
                Estado = x.Estado.ToString().ToUpperInvariant(),
                x.Total
            })
            .ToListAsync(ct);

        return Ok(items);
    }

    [HttpGet("jobs")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetJobs(CancellationToken ct)
    {
        var tipos = new[] { TipoProcesoIntegracion.FacturacionMasiva, TipoProcesoIntegracion.FacturacionAutomatica };
        var jobs = await db.ProcesosIntegracionJobs.AsNoTracking()
            .Where(x => tipos.Contains(x.Tipo))
            .OrderByDescending(x => x.FechaInicio)
            .ThenByDescending(x => x.Id)
            .Select(x => new
            {
                x.Id,
                Tipo = x.Tipo.ToString().ToUpperInvariant(),
                x.Nombre,
                x.ClaveIdempotencia,
                Estado = x.Estado.ToString().ToUpperInvariant(),
                x.FechaInicio,
                x.FechaFin,
                x.TotalRegistros,
                x.RegistrosProcesados,
                x.RegistrosExitosos,
                x.RegistrosConError,
                x.Observacion
            })
            .ToListAsync(ct);

        return Ok(jobs);
    }

    [HttpGet("jobs/{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetJobById(long id, CancellationToken ct)
    {
        var job = await db.ProcesosIntegracionJobs.AsNoTracking()
            .Where(x => x.Id == id && (x.Tipo == TipoProcesoIntegracion.FacturacionMasiva || x.Tipo == TipoProcesoIntegracion.FacturacionAutomatica))
            .FirstOrDefaultAsync(ct);

        if (job is null)
            return NotFound(new { error = $"No se encontró el job de facturación ID {id}." });

        var logs = await db.ProcesosIntegracionLogs.AsNoTracking()
            .Where(x => x.JobId == id)
            .OrderBy(x => x.Id)
            .ToListAsync(ct);

        return Ok(new
        {
            job.Id,
            Tipo = job.Tipo.ToString().ToUpperInvariant(),
            job.Nombre,
            job.ClaveIdempotencia,
            Estado = job.Estado.ToString().ToUpperInvariant(),
            job.FechaInicio,
            job.FechaFin,
            job.TotalRegistros,
            job.RegistrosProcesados,
            job.RegistrosExitosos,
            job.RegistrosConError,
            job.PayloadResumen,
            job.Observacion,
            Logs = logs.Select(x => new
            {
                x.Id,
                Nivel = x.Nivel.ToString().ToUpperInvariant(),
                x.Mensaje,
                x.Referencia,
                x.Payload,
                x.CreatedAt
            })
        });
    }

    [HttpPost("programaciones/automatica")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ProgramarAutomatica([FromBody] ProgramarFacturacionAutomaticaRequest request, CancellationToken ct)
    {
        var programacion = await schedulerService.ProgramarFacturacionAutomaticaAsync(
            new EjecutarFacturacionAutomaticaCommand(
                request.SucursalId,
                request.TipoComprobanteOrigenId,
                request.TipoComprobanteDestinoId,
                request.Desde,
                request.Hasta,
                request.TerceroId,
                request.SoloEmitidos,
                request.PuntoFacturacionId,
                request.Fecha,
                request.FechaVencimiento,
                request.Observacion,
                request.OperacionStock,
                request.OperacionCuentaCorriente,
                request.AutorizarAfip,
                request.UsarCaea,
                request.ClaveIdempotencia),
            request.IntervaloMinutos,
            request.PrimeraEjecucion,
            request.Observacion,
            ct);

        await db.SaveChangesAsync(ct);
        return Ok(new { id = programacion.Id });
    }

    [HttpGet("programaciones")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProgramaciones([FromQuery] bool? activa = null, CancellationToken ct = default)
    {
        var query = db.BatchProgramaciones.AsNoTracking();
        if (activa.HasValue)
            query = query.Where(x => x.Activa == activa.Value);

        var items = await query
            .OrderBy(x => x.ProximaEjecucion)
            .ThenBy(x => x.Id)
            .ToListAsync(ct);

        return Ok(items.Select(x => new
        {
            x.Id,
            Tipo = x.TipoProceso.ToString().ToUpperInvariant(),
            x.Nombre,
            x.IntervaloMinutos,
            x.ProximaEjecucion,
            x.UltimaEjecucion,
            x.Activa,
            x.Observacion
        }));
    }

    [HttpGet("settings")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSettings(CancellationToken ct)
    {
        var settings = await settingsService.ResolveAsync(ct);
        return Ok(new
        {
            settings.SchedulerHabilitado,
            settings.SchedulerPollSeconds,
            settings.SchedulerLote,
            settings.SchedulerReintentoErrorMinutos,
            settings.SchedulerQueueMode,
            settings.ParsersHabilitados,
            settings.LayoutLegacyProfile
        });
    }

    [HttpGet("dashboard-operativo")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDashboardOperativo(CancellationToken ct)
    {
        var ahora = DateTimeOffset.UtcNow;
        var programaciones = await db.BatchProgramaciones.AsNoTracking().Where(x => !x.IsDeleted).ToListAsync(ct);
        var jobs = await db.ProcesosIntegracionJobs.AsNoTracking()
            .Where(x => x.Tipo == TipoProcesoIntegracion.FacturacionMasiva || x.Tipo == TipoProcesoIntegracion.FacturacionAutomatica)
            .ToListAsync(ct);

        return Ok(new
        {
            Programaciones = new
            {
                Total = programaciones.Count,
                Activas = programaciones.Count(x => x.Activa),
                Inactivas = programaciones.Count(x => !x.Activa),
                Vencidas = programaciones.Count(x => x.Activa && x.ProximaEjecucion <= ahora),
                SinEjecucion = programaciones.Count(x => !x.UltimaEjecucion.HasValue)
            },
            Jobs = new
            {
                Total = jobs.Count,
                Pendientes = jobs.Count(x => x.Estado == EstadoProcesoIntegracion.Pendiente),
                EnProceso = jobs.Count(x => x.Estado == EstadoProcesoIntegracion.EnProceso),
                Finalizados = jobs.Count(x => x.Estado == EstadoProcesoIntegracion.Finalizado),
                FinalizadosConErrores = jobs.Count(x => x.Estado == EstadoProcesoIntegracion.FinalizadoConErrores),
                Fallidos = jobs.Count(x => x.Estado == EstadoProcesoIntegracion.Fallido)
            },
            TopObservaciones = jobs.Where(x => !string.IsNullOrWhiteSpace(x.Observacion))
                .GroupBy(x => x.Observacion)
                .Select(g => new { Observacion = g.Key, Cantidad = g.Count() })
                .OrderByDescending(x => x.Cantidad)
                .Take(10)
        });
    }

    [HttpGet("programaciones/resumen")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetResumenProgramaciones(CancellationToken ct)
    {
        var items = await db.BatchProgramaciones.AsNoTracking().ToListAsync(ct);
        return Ok(new
        {
            Cantidad = items.Count,
            Activas = items.Count(x => x.Activa),
            Inactivas = items.Count(x => !x.Activa),
            Vencidas = items.Count(x => x.Activa && x.ProximaEjecucion <= DateTimeOffset.UtcNow),
            ConEjecucion = items.Count(x => x.UltimaEjecucion.HasValue)
        });
    }

    [HttpPost("programaciones/procesar-pendientes")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ProcesarPendientes(CancellationToken ct)
    {
        var procesados = await schedulerService.ProcesarPendientesAsync(ct);
        return Ok(new { procesados });
    }

    [HttpPost("programaciones/{id:long}/desactivar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> DesactivarProgramacion(long id, [FromBody] AdministrarProgramacionBatchRequest request, CancellationToken ct)
    {
        await schedulerService.DesactivarAsync(id, request.Observacion, ct);
        await db.SaveChangesAsync(ct);
        return Ok();
    }

    [HttpPost("programaciones/{id:long}/reactivar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ReactivarProgramacion(long id, [FromBody] ReactivarProgramacionBatchRequest request, CancellationToken ct)
    {
        await schedulerService.ReactivarAsync(id, request.ProximaEjecucion, request.Observacion, ct);
        await db.SaveChangesAsync(ct);
        return Ok();
    }

    [HttpPut("programaciones/{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateProgramacion(long id, [FromBody] UpdateProgramacionBatchRequest request, CancellationToken ct)
    {
        await schedulerService.ActualizarProgramacionAsync(id, request.IntervaloMinutos, request.ProximaEjecucion, request.Observacion, ct);
        await db.SaveChangesAsync(ct);
        return Ok();
    }
}

public record ProgramarFacturacionAutomaticaRequest(
    long SucursalId,
    long TipoComprobanteOrigenId,
    long TipoComprobanteDestinoId,
    DateOnly Desde,
    DateOnly Hasta,
    long? TerceroId,
    bool SoloEmitidos,
    long? PuntoFacturacionId,
    DateOnly Fecha,
    DateOnly? FechaVencimiento,
    string? Observacion,
    ZuluIA_Back.Application.Features.Ventas.Common.OperacionStockVenta OperacionStock,
    ZuluIA_Back.Application.Features.Ventas.Common.OperacionCuentaCorrienteVenta OperacionCuentaCorriente,
    bool AutorizarAfip,
    bool UsarCaea,
    string? ClaveIdempotencia,
    int IntervaloMinutos,
    DateTimeOffset PrimeraEjecucion);
public record AdministrarProgramacionBatchRequest(string? Observacion);
public record ReactivarProgramacionBatchRequest(DateTimeOffset ProximaEjecucion, string? Observacion);
public record UpdateProgramacionBatchRequest(int IntervaloMinutos, DateTimeOffset ProximaEjecucion, string? Observacion);
