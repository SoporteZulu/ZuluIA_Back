using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ZuluIA_Back.Api.Security;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Integraciones.Services;
using ZuluIA_Back.Application.Features.Reportes.Enums;
using ZuluIA_Back.Application.Features.Reportes.Services;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Api.Controllers;

[ApiController]
[Authorize]
[RequirePermission("OPERACIONES.DASHBOARD")]
[AuditCriticalOperation("OPERACIONES_OBSERVABILIDAD")]
[Route("api/[controller]")]
[Produces("application/json")]
public class OperacionesController(
    IMediator mediator,
    IApplicationDbContext db,
    ISeguridadRepository seguridadRepo,
    IServiceProvider serviceProvider,
    EndpointDataSource endpointDataSource) : BaseController(mediator)
{
    private OperacionesBatchSettingsService batchSettingsService => serviceProvider.GetRequiredService<OperacionesBatchSettingsService>();
    private ExternalIntegrationProviderSettingsService providerSettingsService => serviceProvider.GetRequiredService<ExternalIntegrationProviderSettingsService>();
    private GoLiveOperativoReadinessService goLiveOperativoReadinessService => serviceProvider.GetRequiredService<GoLiveOperativoReadinessService>();
    private FiscalHardwareDiagnosticService fiscalHardwareDiagnosticService => serviceProvider.GetRequiredService<FiscalHardwareDiagnosticService>();
    private ZuluAppReplacementCertificationService zuluAppReplacementCertificationService => serviceProvider.GetRequiredService<ZuluAppReplacementCertificationService>();
    private ReporteExportacionService reporteExportacionService => serviceProvider.GetRequiredService<ReporteExportacionService>();

    [HttpGet("resumen-transversal")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetResumenTransversal(CancellationToken ct)
    {
        var ahora = DateTimeOffset.UtcNow;
        return Ok(new
        {
            UsuariosActivos = await db.Usuarios.AsNoTracking().CountAsync(x => x.Activo && !x.IsDeleted, ct),
            UsuariosInactivos = await db.Usuarios.AsNoTracking().CountAsync(x => !x.Activo || x.IsDeleted, ct),
            Permisos = await db.Seguridad.AsNoTracking().CountAsync(ct),
            JobsPendientes = await db.ProcesosIntegracionJobs.AsNoTracking().CountAsync(x => x.Estado == ZuluIA_Back.Domain.Enums.EstadoProcesoIntegracion.Pendiente, ct),
            JobsFallidos = await db.ProcesosIntegracionJobs.AsNoTracking().CountAsync(x => x.Estado == ZuluIA_Back.Domain.Enums.EstadoProcesoIntegracion.Fallido, ct),
            ProgramacionesActivas = await db.BatchProgramaciones.AsNoTracking().CountAsync(x => x.Activa && !x.IsDeleted, ct),
            ProgramacionesVencidas = await db.BatchProgramaciones.AsNoTracking().CountAsync(x => x.Activa && !x.IsDeleted && x.ProximaEjecucion <= ahora, ct),
            SpoolPendiente = await db.ImpresionSpoolTrabajos.AsNoTracking().CountAsync(x => x.Estado == ZuluIA_Back.Domain.Enums.EstadoImpresionSpool.Pendiente && !x.IsDeleted, ct),
            SpoolConError = await db.ImpresionSpoolTrabajos.AsNoTracking().CountAsync(x => x.Estado == ZuluIA_Back.Domain.Enums.EstadoImpresionSpool.Error && !x.IsDeleted, ct),
            IntegracionesExternasConError = await db.IntegracionesExternasAudit.AsNoTracking().CountAsync(x => !x.Exitoso, ct),
            IntegracionesExternasFuncionales = await db.IntegracionesExternasAudit.AsNoTracking().CountAsync(x => x.ErrorFuncional, ct),
            AfipErrores = await db.AfipWsfeAudits.AsNoTracking().CountAsync(x => !x.Exitoso, ct),
            CartasPorteConError = await db.CartasPorte.AsNoTracking().CountAsync(x => x.UltimoErrorCtg != null, ct),
            ChequesAuditados = await db.ChequesHistorial.AsNoTracking().CountAsync(ct)
        });
    }

    [HttpGet("auditoria-operativa")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAuditoriaOperativa([FromQuery] int top = 50, CancellationToken ct = default)
    {
        var take = Math.Clamp(top, 1, 200);

        var logsIntegracion = await db.ProcesosIntegracionLogs.AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .Take(take)
            .Select(x => new AuditoriaOperativaItemDto(
                x.CreatedAt,
                "INTEGRACION",
                x.Nivel.ToString().ToUpperInvariant(),
                x.Referencia ?? $"JOB:{x.JobId}",
                x.Mensaje,
                x.Payload))
            .ToListAsync(ct);

        var auditoriasExternas = await db.IntegracionesExternasAudit.AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .Take(take)
            .Select(x => new AuditoriaOperativaItemDto(
                x.CreatedAt,
                $"EXTERNA:{x.Proveedor.ToString().ToUpperInvariant()}",
                x.Exitoso ? "OK" : x.ErrorFuncional ? "FUNCIONAL" : "ERROR",
                x.ReferenciaTipo == null ? x.Operacion : x.ReferenciaTipo + ":" + x.ReferenciaId,
                x.MensajeError ?? x.Operacion,
                x.CodigoError))
            .ToListAsync(ct);

        var auditoriasAfip = await db.AfipWsfeAudits.AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .Take(take)
            .Select(x => new AuditoriaOperativaItemDto(
                x.CreatedAt,
                "AFIP",
                x.Exitoso ? "OK" : "ERROR",
                $"COMPROBANTE:{x.ComprobanteId}",
                x.MensajeError ?? x.Operacion.ToString().ToUpperInvariant(),
                x.Cae ?? x.Caea))
            .ToListAsync(ct);

        var auditoriasCheques = await db.ChequesHistorial.AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .Take(take)
            .Select(x => new AuditoriaOperativaItemDto(
                x.CreatedAt,
                "CHEQUES",
                x.EstadoNuevo.ToString().ToUpperInvariant(),
                $"CHEQUE:{x.ChequeId}",
                x.Observacion ?? x.Operacion.ToString().ToUpperInvariant(),
                x.EstadoAnterior.HasValue ? x.EstadoAnterior.Value.ToString().ToUpperInvariant() : null))
            .ToListAsync(ct);

        var auditoriasCartasPorte = await db.CartasPorteEventos.AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .Take(take)
            .Select(x => new AuditoriaOperativaItemDto(
                x.CreatedAt,
                "CTG",
                x.EstadoNuevo.ToString().ToUpperInvariant(),
                $"CARTA_PORTE:{x.CartaPorteId}",
                x.Mensaje ?? x.TipoEvento.ToString().ToUpperInvariant(),
                x.NroCtg))
            .ToListAsync(ct);

        var items = logsIntegracion
            .Concat(auditoriasExternas)
            .Concat(auditoriasAfip)
            .Concat(auditoriasCheques)
            .Concat(auditoriasCartasPorte)
            .OrderByDescending(x => x.Fecha)
            .Take(take)
            .ToList();

        return Ok(items);
    }

    [HttpGet("usuarios/{usuarioId:long}/perfil-operativo")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPerfilOperativoUsuario(long usuarioId, CancellationToken ct)
    {
        var usuario = await db.Usuarios.AsNoTracking().FirstOrDefaultAsync(x => x.Id == usuarioId && !x.IsDeleted, ct);
        if (usuario is null)
            return NotFound(new { error = $"No se encontró el usuario ID {usuarioId}." });

        var permisos = await seguridadRepo.GetPermisosUsuarioAsync(usuarioId, ct);
        var sucursales = await db.UsuariosSucursal.AsNoTracking().Where(x => x.UsuarioId == usuarioId).Select(x => x.SucursalId).ToListAsync(ct);
        var menuIds = await db.MenuUsuario.AsNoTracking().Where(x => x.UsuarioId == usuarioId).Select(x => x.MenuId).ToListAsync(ct);

        return Ok(new
        {
            usuario.Id,
            usuario.UserName,
            usuario.NombreCompleto,
            usuario.Email,
            usuario.Activo,
            usuario.SupabaseUserId,
            Sucursales = sucursales,
            MenuIds = menuIds,
            CantidadSucursales = sucursales.Count,
            CantidadMenu = menuIds.Count,
            CantidadPermisos = permisos.Count,
            PermisosHabilitados = permisos.Count(x => x.Value),
            PermisosDenegados = permisos.Count(x => !x.Value),
            Permisos = permisos.OrderBy(x => x.Key)
        });
    }

    [HttpGet("jobs-ambiente")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetJobsAmbiente(CancellationToken ct)
    {
        var batch = await batchSettingsService.ResolveAsync(ct);
        var providers = await providerSettingsService.ResolveAllAsync(ct);

        return Ok(new
        {
            Batch = new
            {
                batch.SchedulerHabilitado,
                batch.SchedulerPollSeconds,
                batch.SchedulerLote,
                batch.SchedulerReintentoErrorMinutos,
                batch.SpoolHabilitado,
                batch.SpoolPollSeconds,
                batch.SpoolLote,
                batch.SpoolReintentoMinutos,
                batch.SpoolMaxIntentos
            },
            ProveedoresExternos = providers.Select(x => new
            {
                Proveedor = x.Proveedor.ToString().ToUpperInvariant(),
                x.Ambiente,
                x.Endpoint,
                x.Habilitada,
                CredencialesCompletas = x.HasAnyCredential,
                x.TimeoutMs,
                x.Reintentos,
                x.CircuitThreshold,
                CircuitOpenSeconds = (int)x.CircuitOpenFor.TotalSeconds
            })
        });
    }

    [HttpGet("go-live-operativo")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetGoLiveOperativo(CancellationToken ct)
        => Ok(await goLiveOperativoReadinessService.EvaluateAsync(ct));

    [HttpGet("reemplazo-zuluapp")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetReemplazoZuluApp(CancellationToken ct)
        => Ok(await zuluAppReplacementCertificationService.EvaluateAsync(ct));

    [HttpGet("reemplazo-zuluapp/smoke")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSmokeZuluApp(CancellationToken ct)
        => Ok(await zuluAppReplacementCertificationService.BuildModuleSmokeAsync(ct));

    [HttpGet("reemplazo-zuluapp/paridad")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetParidadZuluApp(CancellationToken ct)
        => Ok(await zuluAppReplacementCertificationService.BuildModuleParityAsync(ct));

    [HttpGet("reemplazo-zuluapp/evidencias")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEvidenciasZuluApp(CancellationToken ct)
        => Ok(await zuluAppReplacementCertificationService.BuildEvidenceChecklistAsync(ct));

    [HttpGet("reemplazo-zuluapp/corte")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPlanCorteZuluApp(CancellationToken ct)
        => Ok(await zuluAppReplacementCertificationService.BuildCutoverPlanAsync(ct));

    [HttpGet("reemplazo-zuluapp/acta")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActaZuluApp(CancellationToken ct)
        => Ok(await zuluAppReplacementCertificationService.BuildGoLiveMinutesTemplateAsync(ct));

    [HttpGet("reemplazo-zuluapp/exportar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportarReemplazoZuluApp([FromQuery] FormatoExportacionReporte formato = FormatoExportacionReporte.Csv, CancellationToken ct = default)
    {
        var reporte = await zuluAppReplacementCertificationService.BuildCertificationReportAsync(ct);
        var archivo = reporteExportacionService.Exportar(reporte, formato, "reemplazo_zuluapp_certificacion");
        return File(archivo.Contenido, archivo.ContentType, archivo.NombreArchivo);
    }

    [HttpGet("dashboard-operativo-final")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDashboardOperativoFinal(CancellationToken ct)
    {
        var ahora = DateTimeOffset.UtcNow;
        var readiness = await goLiveOperativoReadinessService.EvaluateAsync(ct);
        var hardware = await fiscalHardwareDiagnosticService.DiagnoseAsync(ct);

        var jobs = await db.ProcesosIntegracionJobs.AsNoTracking().ToListAsync(ct);
        var spool = await db.ImpresionSpoolTrabajos.AsNoTracking().Where(x => !x.IsDeleted).ToListAsync(ct);
        var programaciones = await db.BatchProgramaciones.AsNoTracking().Where(x => !x.IsDeleted).ToListAsync(ct);
        var monitores = await db.MonitoresExportacion.AsNoTracking().ToListAsync(ct);

        var importTypes = new[]
        {
            ZuluIA_Back.Domain.Enums.TipoProcesoIntegracion.ImportacionClientes,
            ZuluIA_Back.Domain.Enums.TipoProcesoIntegracion.ImportacionVentas,
            ZuluIA_Back.Domain.Enums.TipoProcesoIntegracion.ImportacionNotasPedido,
            ZuluIA_Back.Domain.Enums.TipoProcesoIntegracion.ImportacionOperativa
        };

        return Ok(new
        {
            readiness.ReadyForGoLive,
            Readiness = readiness,
            Batch = new
            {
                ProgramacionesActivas = programaciones.Count(x => x.Activa),
                ProgramacionesVencidas = programaciones.Count(x => x.Activa && x.ProximaEjecucion <= ahora),
                JobsImportacion = jobs.Count(x => importTypes.Contains(x.Tipo)),
                JobsFacturacion = jobs.Count(x => x.Tipo == ZuluIA_Back.Domain.Enums.TipoProcesoIntegracion.FacturacionMasiva || x.Tipo == ZuluIA_Back.Domain.Enums.TipoProcesoIntegracion.FacturacionAutomatica),
                JobsSincronizacion = jobs.Count(x => x.Tipo == ZuluIA_Back.Domain.Enums.TipoProcesoIntegracion.Sincronizacion || x.Tipo == ZuluIA_Back.Domain.Enums.TipoProcesoIntegracion.SincronizacionLegacyEspecifica),
                JobsFallidos = jobs.Count(x => x.Estado == ZuluIA_Back.Domain.Enums.EstadoProcesoIntegracion.Fallido),
                JobsFinalizadosConErrores = jobs.Count(x => x.Estado == ZuluIA_Back.Domain.Enums.EstadoProcesoIntegracion.FinalizadoConErrores)
            },
            Spool = new
            {
                Total = spool.Count,
                Pendientes = spool.Count(x => x.Estado == ZuluIA_Back.Domain.Enums.EstadoImpresionSpool.Pendiente),
                EnProceso = spool.Count(x => x.Estado == ZuluIA_Back.Domain.Enums.EstadoImpresionSpool.EnProceso),
                Completados = spool.Count(x => x.Estado == ZuluIA_Back.Domain.Enums.EstadoImpresionSpool.Completado),
                Errores = spool.Count(x => x.Estado == ZuluIA_Back.Domain.Enums.EstadoImpresionSpool.Error),
                ReintentoVencido = spool.Count(x => x.Estado == ZuluIA_Back.Domain.Enums.EstadoImpresionSpool.Error && x.ProximoIntento.HasValue && x.ProximoIntento <= ahora),
                SinReintento = spool.Count(x => x.Estado == ZuluIA_Back.Domain.Enums.EstadoImpresionSpool.Error && !x.ProximoIntento.HasValue)
            },
            Importacion = new
            {
                Monitores = monitores.Count,
                Pendientes = monitores.Count(x => x.RegistrosPendientes > 0),
                UltimosEstados = monitores.GroupBy(x => x.UltimoEstado)
                    .Select(g => new
                    {
                        Estado = g.Key.HasValue ? g.Key.Value.ToString().ToUpperInvariant() : "SIN_ESTADO",
                        Cantidad = g.Count()
                    })
                    .OrderByDescending(x => x.Cantidad)
            },
            FiscalHardware = hardware,
            Seguridad = new
            {
                EndpointsTotales = endpointDataSource.Endpoints.OfType<RouteEndpoint>().Count(),
                EndpointsAutorizados = endpointDataSource.Endpoints.OfType<RouteEndpoint>().Count(x => x.Metadata.OfType<IAuthorizeData>().Any()),
                EndpointsConPermiso = endpointDataSource.Endpoints.OfType<RouteEndpoint>().Count(x => x.Metadata.OfType<ZuluIA_Back.Api.Security.RequirePermissionAttribute>().Any())
            },
            TopErroresSpool = spool.Where(x => !string.IsNullOrWhiteSpace(x.MensajeError))
                .GroupBy(x => x.MensajeError)
                .Select(g => new { Mensaje = g.Key, Cantidad = g.Count() })
                .OrderByDescending(x => x.Cantidad)
                .Take(10)
        });
    }

    [HttpGet("metricas-operativas")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMetricasOperativas(CancellationToken ct)
    {
        var hoy = DateOnly.FromDateTime(DateTime.Today);
        return Ok(new
        {
            Ventas = new
            {
                Comprobantes = await db.Comprobantes.AsNoTracking().CountAsync(x => !x.IsDeleted, ct),
                EmitidosHoy = await db.Comprobantes.AsNoTracking().CountAsync(x => !x.IsDeleted && x.Fecha == hoy, ct),
                Imputaciones = await db.Imputaciones.AsNoTracking().CountAsync(ct)
            },
            Finanzas = new
            {
                Cobros = await db.Cobros.AsNoTracking().CountAsync(x => !x.IsDeleted, ct),
                Pagos = await db.Pagos.AsNoTracking().CountAsync(x => !x.IsDeleted, ct),
                Cheques = await db.Cheques.AsNoTracking().CountAsync(x => !x.IsDeleted, ct),
                CierresTesoreria = await db.TesoreriaCierres.AsNoTracking().CountAsync(ct)
            },
            Facturacion = new
            {
                AfipAudits = await db.AfipWsfeAudits.AsNoTracking().CountAsync(ct),
                CartasPorte = await db.CartasPorte.AsNoTracking().CountAsync(x => !x.IsDeleted, ct),
                CartasPorteConError = await db.CartasPorte.AsNoTracking().CountAsync(x => x.UltimoErrorCtg != null && !x.IsDeleted, ct)
            },
            Integraciones = new
            {
                Jobs = await db.ProcesosIntegracionJobs.AsNoTracking().CountAsync(ct),
                Logs = await db.ProcesosIntegracionLogs.AsNoTracking().CountAsync(ct),
                Monitores = await db.MonitoresExportacion.AsNoTracking().CountAsync(ct),
                AuditoriasExternas = await db.IntegracionesExternasAudit.AsNoTracking().CountAsync(ct)
            },
            Logistica = new
            {
                OrdenesPreparacion = await db.OrdenesPreparacion.AsNoTracking().CountAsync(x => !x.IsDeleted, ct),
                Transferencias = await db.TransferenciasDeposito.AsNoTracking().CountAsync(x => !x.IsDeleted, ct),
                Eventos = await db.LogisticaInternaEventos.AsNoTracking().CountAsync(x => !x.IsDeleted, ct)
            },
            Colegio = new
            {
                Cedulones = await db.Cedulones.AsNoTracking().CountAsync(x => !x.IsDeleted, ct),
                Lotes = await db.ColegioLotes.AsNoTracking().CountAsync(x => !x.IsDeleted, ct),
                Cobinpro = await db.ColegioCobinproOperaciones.AsNoTracking().CountAsync(x => !x.IsDeleted, ct)
            },
            Contratos = new
            {
                Contratos = await db.Contratos.AsNoTracking().CountAsync(x => !x.IsDeleted, ct),
                Historial = await db.ContratosHistorial.AsNoTracking().CountAsync(x => !x.IsDeleted, ct),
                Impactos = await db.ContratosImpactos.AsNoTracking().CountAsync(x => !x.IsDeleted, ct)
            },
            Produccion = new
            {
                OrdenesTrabajo = await db.OrdenesTrabajo.AsNoTracking().CountAsync(x => !x.IsDeleted, ct),
                OrdenesEmpaque = await db.OrdenesEmpaque.AsNoTracking().CountAsync(x => !x.IsDeleted, ct),
                Formulas = await db.FormulasProduccion.AsNoTracking().CountAsync(x => !x.IsDeleted, ct)
            },
            RRHH = new
            {
                Empleados = await db.Empleados.AsNoTracking().CountAsync(ct),
                Liquidaciones = await db.LiquidacionesSueldo.AsNoTracking().CountAsync(ct),
                Comprobantes = await db.ComprobantesEmpleados.AsNoTracking().CountAsync(x => !x.IsDeleted, ct)
            }
        });
    }

    private sealed record AuditoriaOperativaItemDto(
        DateTimeOffset Fecha,
        string Fuente,
        string Estado,
        string Referencia,
        string Mensaje,
        string? Detalle);
}
