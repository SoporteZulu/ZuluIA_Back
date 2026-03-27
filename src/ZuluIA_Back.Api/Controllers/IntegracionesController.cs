using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Api.Security;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Integraciones.Commands;
using ZuluIA_Back.Application.Features.Integraciones.Services;
using ZuluIA_Back.Application.Features.Reportes.DTOs;
using ZuluIA_Back.Application.Features.Reportes.Enums;
using ZuluIA_Back.Application.Features.Reportes.Services;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Api.Controllers;

[RequirePermission("INTEGRACIONES.OPERAR")]
[AuditCriticalOperation("INTEGRACIONES_OPERATIVAS")]
public class IntegracionesController(IMediator mediator, IApplicationDbContext db, ImportacionArchivoService importacionArchivoService, ArchivoTabularParserService parserService, ReporteExportacionService reporteExportacionService, ExternalIntegrationProviderSettingsService providerSettingsService, ExternalIntegrationCertificationService certificationService, ExternalFiscalPrecheckService fiscalPrecheckService, ExternalFiscalReadinessService fiscalReadinessService, ExternalProviderErrorCatalogService errorCatalogService) : BaseController(mediator)
{
    [HttpGet("importar/layouts")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetImportLayouts([FromServices] ArchivoImportLayoutProfileService layoutProfileService)
        => Ok(layoutProfileService.GetSupportedCircuits().Select(layoutProfileService.GetTemplate));

    [HttpGet("importar/layouts/{circuito}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetImportLayoutByCircuito(string circuito, [FromServices] ArchivoImportLayoutProfileService layoutProfileService)
    {
        try
        {
            return Ok(layoutProfileService.GetTemplate(circuito));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpPost("importar/clientes")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ImportarClientes([FromBody] ImportarClientesCommand command, CancellationToken ct)
        => FromResult(await Mediator.Send(command, ct));

    [HttpPost("importar/clientes/archivo")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ImportarClientesArchivo([FromBody] ImportarArchivoClientesRequest request, CancellationToken ct)
        => FromResult(await importacionArchivoService.ImportarClientesAsync(request.NombreArchivo, request.ContenidoBase64, request.ActualizarExistentes, request.Observacion, ct));

    [HttpPost("importar/ventas")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ImportarVentas([FromBody] ImportarVentasCommand command, CancellationToken ct)
        => FromResult(await Mediator.Send(command, ct));

    [HttpPost("importar/ventas/archivo")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ImportarVentasArchivo([FromBody] ImportarArchivoVentasRequest request, CancellationToken ct)
        => FromResult(await importacionArchivoService.ImportarVentasAsync(request.NombreArchivo, request.ContenidoBase64, request.Observacion, ct));

    [HttpPost("importar/notas-pedido")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ImportarNotasPedido([FromBody] ImportarNotasPedidoCommand command, CancellationToken ct)
        => FromResult(await Mediator.Send(command, ct));

    [HttpPost("importar/operativas")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ImportarOperativas([FromBody] ImportarOperacionesCommand command, CancellationToken ct)
        => FromResult(await Mediator.Send(command, ct));

    [HttpPost("importar/preview")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult PreviewArchivo([FromBody] ImportarArchivoPreviewRequest request)
    {
        try
        {
            var analysis = parserService.Analyze(request.NombreArchivo, Convert.FromBase64String(request.ContenidoBase64), request.MaxRows);
            return Ok(new
            {
                analysis.FileName,
                analysis.Format,
                analysis.LayoutProfile,
                analysis.Separator,
                totalRows = analysis.TotalRows,
                previewRows = analysis.PreviewRows,
                columns = analysis.Columns,
                rows = analysis.Rows,
                supportedFormats = analysis.SupportedFormats
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("importar/validate-layout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult ValidateLayout([FromBody] ImportarArchivoLayoutValidationRequest request, [FromServices] ArchivoImportLayoutValidationService validationService)
    {
        try
        {
            var result = validationService.Validate(request.Circuito, request.NombreArchivo, Convert.FromBase64String(request.ContenidoBase64), request.MaxRows);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("syncs")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> EjecutarSync([FromBody] EjecutarSyncIntegracionCommand command, CancellationToken ct)
        => FromResult(await Mediator.Send(command, ct));

    [HttpPost("exportar/cot")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ExportarCot([FromBody] ExportarCotLegacyCommand command, CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return result.IsSuccess
            ? File(result.Value.Contenido, result.Value.ContentType, result.Value.NombreArchivo)
            : BadRequest(new { error = result.Error });
    }

public record ImportarArchivoClientesRequest(string NombreArchivo, string ContenidoBase64, bool ActualizarExistentes = true, string? Observacion = null);
public record ImportarArchivoVentasRequest(string NombreArchivo, string ContenidoBase64, string? Observacion = null);
public record ImportarArchivoPreviewRequest(string NombreArchivo, string ContenidoBase64, int MaxRows = 20);
public record ImportarArchivoLayoutValidationRequest(string Circuito, string NombreArchivo, string ContenidoBase64, int MaxRows = 20);
public record FiscalPrecheckRequest(string Proveedor, string Operacion, long ReferenciaId, long? TimbradoFiscalId = null, string? CodigoSeguridad = null, string? ReferenciaExterna = null, bool? UsarCaea = null);
public record FiscalReadinessRequest(string Proveedor, string Operacion, long ReferenciaId, long? TimbradoFiscalId = null, string? CodigoSeguridad = null, string? ReferenciaExterna = null, bool? UsarCaea = null, bool TestConectividad = true);

    [HttpPost("exportar/zgcot")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ExportarZgcot([FromBody] ExportarZgcotLegacyCommand command, CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return result.IsSuccess
            ? File(result.Value.Contenido, result.Value.ContentType, result.Value.NombreArchivo)
            : BadRequest(new { error = result.Error });
    }

    [HttpPost("syncs/legacy")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> EjecutarSyncLegacy([FromBody] EjecutarSyncLegacyEspecificoCommand command, CancellationToken ct)
        => FromResult(await Mediator.Send(command, ct));

    [HttpGet("jobs")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetJobs([FromQuery] TipoProcesoIntegracion? tipo = null, [FromQuery] EstadoProcesoIntegracion? estado = null, [FromQuery] string? search = null, [FromQuery] DateTimeOffset? desde = null, [FromQuery] DateTimeOffset? hasta = null, CancellationToken ct = default)
    {
        var query = db.ProcesosIntegracionJobs.AsNoTracking();
        if (tipo.HasValue)
            query = query.Where(x => x.Tipo == tipo.Value);
        if (estado.HasValue)
            query = query.Where(x => x.Estado == estado.Value);
        if (desde.HasValue)
            query = query.Where(x => x.FechaInicio >= desde.Value);
        if (hasta.HasValue)
            query = query.Where(x => x.FechaInicio <= hasta.Value);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToUpperInvariant();
            query = query.Where(x => x.Nombre.ToUpper().Contains(term)
                || (x.ClaveIdempotencia != null && x.ClaveIdempotencia.Contains(term))
                || (x.Observacion != null && x.Observacion.ToUpper().Contains(term)));
        }

        var items = await query.OrderByDescending(x => x.FechaInicio).ThenByDescending(x => x.Id).ToListAsync(ct);
        return Ok(items.Select(x => new
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
        }));
    }

    [HttpGet("jobs/{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetJobById(long id, CancellationToken ct)
    {
        var job = await db.ProcesosIntegracionJobs.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        if (job is null)
            return NotFound(new { error = $"No se encontró el job de integración ID {id}." });

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

    [HttpGet("monitores-exportacion")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMonitoresExportacion([FromQuery] string? codigo = null, [FromQuery] EstadoProcesoIntegracion? estado = null, CancellationToken ct = default)
    {
        var query = db.MonitoresExportacion.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(codigo))
        {
            var term = codigo.Trim().ToUpperInvariant();
            query = query.Where(x => x.Codigo.Contains(term) || x.Descripcion.ToUpper().Contains(term));
        }
        if (estado.HasValue)
            query = query.Where(x => x.UltimoEstado == estado.Value);

        var items = await query
            .OrderBy(x => x.Codigo)
            .ToListAsync(ct);

        return Ok(items.Select(x => new
        {
            x.Id,
            x.Codigo,
            x.Descripcion,
            x.UltimoJobId,
            x.UltimaEjecucion,
            UltimoEstado = x.UltimoEstado.HasValue ? x.UltimoEstado.Value.ToString().ToUpperInvariant() : null,
            x.RegistrosPendientes,
            x.UltimoMensaje
        }));
    }

    [HttpGet("monitores-exportacion/{codigo}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMonitorByCodigo(string codigo, CancellationToken ct)
    {
        var key = codigo.Trim().ToUpperInvariant();
        var item = await db.MonitoresExportacion.AsNoTracking().FirstOrDefaultAsync(x => x.Codigo == key, ct);
        if (item is null)
            return NotFound(new { error = $"No se encontró el monitor '{codigo}'." });

        return Ok(new
        {
            item.Id,
            item.Codigo,
            item.Descripcion,
            item.UltimoJobId,
            item.UltimaEjecucion,
            UltimoEstado = item.UltimoEstado.HasValue ? item.UltimoEstado.Value.ToString().ToUpperInvariant() : null,
            item.RegistrosPendientes,
            item.UltimoMensaje
        });
    }

    [HttpGet("monitores-exportacion/legacy")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMonitoresLegacy(CancellationToken ct)
    {
        var items = await db.MonitoresExportacion.AsNoTracking()
            .Where(x => x.Codigo.StartsWith("EXPORT_") || x.Codigo.StartsWith("SYNC_"))
            .OrderBy(x => x.Codigo)
            .ToListAsync(ct);

        return Ok(items.Select(x => new
        {
            x.Id,
            x.Codigo,
            x.Descripcion,
            x.UltimoJobId,
            x.UltimaEjecucion,
            UltimoEstado = x.UltimoEstado.HasValue ? x.UltimoEstado.Value.ToString().ToUpperInvariant() : null,
            x.RegistrosPendientes,
            x.UltimoMensaje
        }));
    }

    [HttpGet("auditorias-externas")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAuditoriasExternas([FromQuery] ProveedorIntegracionExterna? proveedor = null, [FromQuery] long? referenciaId = null, [FromQuery] bool? errorFuncional = null, CancellationToken ct = default)
    {
        var query = db.IntegracionesExternasAudit.AsNoTracking();
        if (proveedor.HasValue)
            query = query.Where(x => x.Proveedor == proveedor.Value);
        if (referenciaId.HasValue)
            query = query.Where(x => x.ReferenciaId == referenciaId.Value);
        if (errorFuncional.HasValue)
            query = query.Where(x => x.ErrorFuncional == errorFuncional.Value);

        var items = await query.OrderByDescending(x => x.CreatedAt).ThenByDescending(x => x.Id).ToListAsync(ct);
        return Ok(items.Select(x => new
        {
            x.Id,
            Proveedor = x.Proveedor.ToString().ToUpperInvariant(),
            x.Operacion,
            x.Ambiente,
            x.Endpoint,
            x.ReferenciaTipo,
            x.ReferenciaId,
            x.Exitoso,
            x.Reintentos,
            x.TimeoutMs,
            x.CircuitBreakerAbierto,
            x.DuracionMs,
            x.CodigoError,
            x.ErrorFuncional,
            x.MensajeError,
            x.CreatedAt
        }));
    }

    [HttpGet("configuracion-externa")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetConfiguracionExterna(CancellationToken ct)
    {
        var items = await providerSettingsService.ResolveAllAsync(ct);
        return Ok(items.Select(x => new
        {
            Proveedor = x.Proveedor.ToString().ToUpperInvariant(),
            x.Ambiente,
            x.Endpoint,
            x.Habilitada,
            UsaTransporteReal = !string.IsNullOrWhiteSpace(x.Endpoint) && !x.Endpoint.Contains(".local", StringComparison.OrdinalIgnoreCase),
            CredencialesCompletas = x.HasAnyCredential,
            x.TimeoutMs,
            x.Reintentos,
            x.CircuitThreshold,
            CircuitOpenSeconds = (int)x.CircuitOpenFor.TotalSeconds
        }));
    }

    [HttpGet("certificacion-fiscal")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCertificacionFiscal(CancellationToken ct)
        => Ok(await certificationService.GetCertificationStatusAsync(ct));

    [HttpGet("certificacion-fiscal/operaciones")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCertificacionFiscalOperaciones(CancellationToken ct)
    {
        var operaciones = new[]
        {
            new { Proveedor = ProveedorIntegracionExterna.AfipWsfe, Operacion = "SOLICITARCAE", Path = "solicitar-cae" },
            new { Proveedor = ProveedorIntegracionExterna.AfipWsfe, Operacion = "SOLICITARCAEA", Path = "solicitar-caea" },
            new { Proveedor = ProveedorIntegracionExterna.AfipWsfe, Operacion = "CONSULTARCOMPROBANTE", Path = "consultar" },
            new { Proveedor = ProveedorIntegracionExterna.AfipWsfe, Operacion = "REFRESHESTADO", Path = "refresh" },
            new { Proveedor = ProveedorIntegracionExterna.Ctg, Operacion = "SOLICITAR_CTG", Path = "ctg/solicitar" },
            new { Proveedor = ProveedorIntegracionExterna.Ctg, Operacion = "SOLICITAR_CTG_REINTENTO", Path = "ctg/solicitar-reintento" },
            new { Proveedor = ProveedorIntegracionExterna.Ctg, Operacion = "CONSULTAR_CTG", Path = "ctg/consultar" },
            new { Proveedor = ProveedorIntegracionExterna.Sifen, Operacion = "PROCESAR_COMPROBANTE", Path = "sifen/comprobantes" },
            new { Proveedor = ProveedorIntegracionExterna.Deuce, Operacion = "PROCESAR_COMPROBANTE", Path = "deuce/comprobantes" }
        };

        var items = new List<object>(operaciones.Length);
        foreach (var item in operaciones)
        {
            var provider = await providerSettingsService.ResolveAsync(item.Proveedor, ct);
            var operation = await providerSettingsService.ResolveOperationAsync(item.Proveedor, item.Operacion, item.Path, ct);
            items.Add(new
            {
                Proveedor = item.Proveedor.ToString().ToUpperInvariant(),
                operation.Operation,
                provider.Ambiente,
                provider.Endpoint,
                operation.Path,
                operation.TimeoutMs,
                operation.Reintentos,
                operation.CircuitThreshold,
                CircuitOpenSeconds = (int)operation.CircuitOpenFor.TotalSeconds,
                provider.Habilitada,
                CredencialesCompletas = provider.HasAnyCredential
            });
        }

        return Ok(items);
    }

    [HttpGet("certificacion-fiscal/catalogo-errores")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetCatalogoErroresFiscal()
        => Ok(errorCatalogService.GetCatalog());

    [HttpPost("certificacion-fiscal/{proveedor}/test-conectividad")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> TestConectividad(string proveedor, CancellationToken ct)
    {
        if (!Enum.TryParse<ProveedorIntegracionExterna>(proveedor, true, out var proveedorEnum))
            return BadRequest(new { error = "Proveedor inválido." });

        return Ok(await certificationService.TestConnectivityAsync(proveedorEnum, ct));
    }

    [HttpPost("certificacion-fiscal/precheck")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PrecheckFiscal([FromBody] FiscalPrecheckRequest request, CancellationToken ct)
    {
        if (!Enum.TryParse<ProveedorIntegracionExterna>(request.Proveedor, true, out var proveedorEnum))
            return BadRequest(new { error = "Proveedor inválido." });

        var result = await fiscalPrecheckService.PrecheckAsync(
            proveedorEnum,
            request.Operacion,
            request.ReferenciaId,
            request.TimbradoFiscalId,
            request.CodigoSeguridad,
            request.ReferenciaExterna,
            request.UsarCaea,
            ct);

        return Ok(result);
    }

    [HttpPost("certificacion-fiscal/readiness")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ReadinessFiscal([FromBody] FiscalReadinessRequest request, CancellationToken ct)
    {
        if (!Enum.TryParse<ProveedorIntegracionExterna>(request.Proveedor, true, out var proveedorEnum))
            return BadRequest(new { error = "Proveedor inválido." });

        var defaultPath = GetDefaultOperationPath(proveedorEnum, request.Operacion);
        var result = await fiscalReadinessService.EvaluateAsync(
            proveedorEnum,
            request.Operacion,
            defaultPath,
            request.ReferenciaId,
            request.TimbradoFiscalId,
            request.CodigoSeguridad,
            request.ReferenciaExterna,
            request.UsarCaea,
            request.TestConectividad,
            ct);

        return Ok(result);
    }

    [HttpGet("resumen")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetResumen(CancellationToken ct)
    {
        var jobs = await db.ProcesosIntegracionJobs.AsNoTracking().ToListAsync(ct);
        var monitores = await db.MonitoresExportacion.AsNoTracking().ToListAsync(ct);

        return Ok(new
        {
            Jobs = jobs.Count,
            Pendientes = jobs.Count(x => x.Estado == EstadoProcesoIntegracion.Pendiente),
            EnProceso = jobs.Count(x => x.Estado == EstadoProcesoIntegracion.EnProceso),
            Finalizados = jobs.Count(x => x.Estado == EstadoProcesoIntegracion.Finalizado),
            FinalizadosConErrores = jobs.Count(x => x.Estado == EstadoProcesoIntegracion.FinalizadoConErrores),
            Fallidos = jobs.Count(x => x.Estado == EstadoProcesoIntegracion.Fallido),
            Monitores = monitores.Count,
            MonitoresPendientes = monitores.Count(x => x.RegistrosPendientes > 0)
        });
    }

    private static string GetDefaultOperationPath(ProveedorIntegracionExterna proveedor, string operacion)
    {
        var key = operacion.Trim().ToUpperInvariant();
        return proveedor switch
        {
            ProveedorIntegracionExterna.AfipWsfe when key == "SOLICITARCAE" => "solicitar-cae",
            ProveedorIntegracionExterna.AfipWsfe when key == "SOLICITARCAEA" => "solicitar-caea",
            ProveedorIntegracionExterna.AfipWsfe when key == "CONSULTARCOMPROBANTE" => "consultar",
            ProveedorIntegracionExterna.AfipWsfe when key == "REFRESHESTADO" => "refresh",
            ProveedorIntegracionExterna.Ctg when key == "SOLICITAR_CTG" => "ctg/solicitar",
            ProveedorIntegracionExterna.Ctg when key == "SOLICITAR_CTG_REINTENTO" => "ctg/solicitar-reintento",
            ProveedorIntegracionExterna.Ctg when key == "CONSULTAR_CTG" => "ctg/consultar",
            ProveedorIntegracionExterna.Sifen when key == "PROCESAR_COMPROBANTE" => "sifen/comprobantes",
            ProveedorIntegracionExterna.Deuce when key == "PROCESAR_COMPROBANTE" => "deuce/comprobantes",
            _ => string.Empty
        };
    }

    [HttpGet("reportes/jobs")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportarReporteJobs([FromQuery] FormatoExportacionReporte formato = FormatoExportacionReporte.Csv, CancellationToken ct = default)
    {
        var items = await db.ProcesosIntegracionJobs.AsNoTracking().OrderByDescending(x => x.FechaInicio).ToListAsync(ct);
        var reporte = new ReporteTabularDto
        {
            Titulo = "Jobs de Integración",
            Columnas = ["Id", "Tipo", "Nombre", "ClaveIdempotencia", "Estado", "Inicio", "Fin", "Procesados", "Exitosos", "Errores"],
            Filas = items.Select(x => (IReadOnlyList<string>)new[]
            {
                x.Id.ToString(),
                x.Tipo.ToString().ToUpperInvariant(),
                x.Nombre,
                x.ClaveIdempotencia ?? "—",
                x.Estado.ToString().ToUpperInvariant(),
                x.FechaInicio.ToString("yyyy-MM-dd HH:mm:ss"),
                x.FechaFin?.ToString("yyyy-MM-dd HH:mm:ss") ?? "—",
                x.RegistrosProcesados.ToString(),
                x.RegistrosExitosos.ToString(),
                x.RegistrosConError.ToString()
            }).ToList()
        };

        var archivo = reporteExportacionService.Exportar(reporte, formato, "integraciones_jobs");
        return File(archivo.Contenido, archivo.ContentType, archivo.NombreArchivo);
    }

    [HttpGet("reportes/monitores")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportarReporteMonitores([FromQuery] FormatoExportacionReporte formato = FormatoExportacionReporte.Csv, CancellationToken ct = default)
    {
        var items = await db.MonitoresExportacion.AsNoTracking().OrderBy(x => x.Codigo).ToListAsync(ct);
        var reporte = new ReporteTabularDto
        {
            Titulo = "Monitores de Exportación",
            Columnas = ["Codigo", "Descripcion", "UltimoJobId", "UltimaEjecucion", "UltimoEstado", "Pendientes", "Mensaje"],
            Filas = items.Select(x => (IReadOnlyList<string>)new[]
            {
                x.Codigo,
                x.Descripcion,
                x.UltimoJobId?.ToString() ?? "—",
                x.UltimaEjecucion?.ToString("yyyy-MM-dd HH:mm:ss") ?? "—",
                x.UltimoEstado?.ToString().ToUpperInvariant() ?? "—",
                x.RegistrosPendientes.ToString(),
                x.UltimoMensaje ?? "—"
            }).ToList()
        };

        var archivo = reporteExportacionService.Exportar(reporte, formato, "integraciones_monitores");
        return File(archivo.Contenido, archivo.ContentType, archivo.NombreArchivo);
    }
}
