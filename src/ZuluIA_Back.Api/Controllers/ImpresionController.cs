using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Api.Security;
using ZuluIA_Back.Application.Features.Facturacion.Queries;
using ZuluIA_Back.Application.Features.Impresion.Commands;
using ZuluIA_Back.Application.Features.Impresion.Enums;
using ZuluIA_Back.Application.Features.Integraciones.Services;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Impresion.Services;

namespace ZuluIA_Back.Api.Controllers;

[RequirePermission("IMPRESION.OPERAR")]
[AuditCriticalOperation("IMPRESION_OPERATIVA")]
public class ImpresionController(
    IMediator mediator,
    ImpresionDocumentosService impresionDocumentosService,
    ImpresionSpoolService spoolService,
    FiscalHardwareDiagnosticService fiscalHardwareDiagnosticService,
    OperacionesBatchSettingsService settingsService,
    IApplicationDbContext db)
    : BaseController(mediator)
{
    [HttpGet("comprobantes/{id:long}/pdf")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPdfComprobante(long id, CancellationToken ct)
    {
        try
        {
            var pdf = await impresionDocumentosService.GenerarPdfComprobanteAsync(id, ct);
            return File(pdf.Contenido, pdf.ContentType, pdf.NombreArchivo);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpPost("comprobantes/{id:long}/fiscal")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ImprimirFiscal(long id, [FromBody] ImprimirFiscalRequest request, CancellationToken ct)
        => FromResult(await Mediator.Send(new ImprimirComprobanteFiscalCommand(id, request.Marca), ct));

    [HttpPost("comprobantes/{id:long}/spool/fiscal")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> EncolarImpresionFiscal(long id, [FromBody] ImprimirFiscalRequest request, CancellationToken ct)
    {
        var trabajo = await spoolService.EncolarFiscalAsync(id, request.Marca, ct);
        await db.SaveChangesAsync(ct);
        return Ok(new { id = trabajo.Id });
    }

    [HttpGet("spool")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSpool([FromQuery] long? comprobanteId = null, [FromQuery] string? estado = null, CancellationToken ct = default)
    {
        ZuluIA_Back.Domain.Enums.EstadoImpresionSpool? estadoEnum = null;
        if (!string.IsNullOrWhiteSpace(estado) && Enum.TryParse<ZuluIA_Back.Domain.Enums.EstadoImpresionSpool>(estado, true, out var parsed))
            estadoEnum = parsed;

        var query = db.ImpresionSpoolTrabajos.AsNoTracking();
        if (comprobanteId.HasValue)
            query = query.Where(x => x.ComprobanteId == comprobanteId.Value);
        if (estadoEnum.HasValue)
            query = query.Where(x => x.Estado == estadoEnum.Value);

        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.Id)
            .ToListAsync(ct);

        return Ok(items.Select(x => new
        {
            x.Id,
            x.ComprobanteId,
            x.TipoTrabajo,
            x.Destino,
            Estado = x.Estado.ToString().ToUpperInvariant(),
            x.Intentos,
            x.ProximoIntento,
            x.MensajeError,
            x.CreatedAt
        }));
    }

    [HttpPost("spool/procesar-pendientes")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ProcesarSpoolPendiente(CancellationToken ct)
    {
        var procesados = await spoolService.ProcesarPendientesAsync(ct);
        await db.SaveChangesAsync(ct);
        return Ok(new { procesados });
    }

    [HttpPost("spool/{id:long}/reencolar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ReencolarTrabajo(long id, CancellationToken ct)
    {
        await spoolService.ReencolarAsync(id, ct);
        await db.SaveChangesAsync(ct);
        return Ok();
    }

    [HttpGet("spool/resumen")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetResumenSpool(CancellationToken ct)
    {
        var items = await db.ImpresionSpoolTrabajos.AsNoTracking().ToListAsync(ct);
        return Ok(new
        {
            Cantidad = items.Count,
            Pendientes = items.Count(x => x.Estado == ZuluIA_Back.Domain.Enums.EstadoImpresionSpool.Pendiente),
            EnProceso = items.Count(x => x.Estado == ZuluIA_Back.Domain.Enums.EstadoImpresionSpool.EnProceso),
            Completados = items.Count(x => x.Estado == ZuluIA_Back.Domain.Enums.EstadoImpresionSpool.Completado),
            Errores = items.Count(x => x.Estado == ZuluIA_Back.Domain.Enums.EstadoImpresionSpool.Error),
            SinReintento = items.Count(x => x.Estado == ZuluIA_Back.Domain.Enums.EstadoImpresionSpool.Error && !x.ProximoIntento.HasValue)
        });
    }

    [HttpGet("spool/settings")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSpoolSettings(CancellationToken ct)
    {
        var settings = await settingsService.ResolveAsync(ct);
        return Ok(new
        {
            settings.SpoolHabilitado,
            settings.SpoolPollSeconds,
            settings.SpoolLote,
            settings.SpoolReintentoMinutos,
            settings.SpoolMaxIntentos,
            settings.SpoolBackoffFactor,
            settings.SpoolMaxRetryMinutes,
            settings.SpoolQueueMode,
            settings.ImpresionFiscalHabilitada,
            settings.EpsonHabilitada,
            settings.HasarHabilitada,
            settings.PdfLayoutProfile
        });
    }

    [HttpGet("spool/dashboard-operativo")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDashboardOperativoSpool(CancellationToken ct)
    {
        var ahora = DateTimeOffset.UtcNow;
        var items = await db.ImpresionSpoolTrabajos.AsNoTracking().Where(x => !x.IsDeleted).ToListAsync(ct);
        return Ok(new
        {
            Total = items.Count,
            Pendientes = items.Count(x => x.Estado == ZuluIA_Back.Domain.Enums.EstadoImpresionSpool.Pendiente),
            EnProceso = items.Count(x => x.Estado == ZuluIA_Back.Domain.Enums.EstadoImpresionSpool.EnProceso),
            Completados = items.Count(x => x.Estado == ZuluIA_Back.Domain.Enums.EstadoImpresionSpool.Completado),
            Errores = items.Count(x => x.Estado == ZuluIA_Back.Domain.Enums.EstadoImpresionSpool.Error),
            SinReintento = items.Count(x => x.Estado == ZuluIA_Back.Domain.Enums.EstadoImpresionSpool.Error && !x.ProximoIntento.HasValue),
            ReintentoVencido = items.Count(x => x.Estado == ZuluIA_Back.Domain.Enums.EstadoImpresionSpool.Error && x.ProximoIntento.HasValue && x.ProximoIntento <= ahora),
            PorDestino = items.GroupBy(x => x.Destino).Select(g => new
            {
                Destino = g.Key,
                Cantidad = g.Count(),
                Errores = g.Count(x => x.Estado == ZuluIA_Back.Domain.Enums.EstadoImpresionSpool.Error),
                Pendientes = g.Count(x => x.Estado == ZuluIA_Back.Domain.Enums.EstadoImpresionSpool.Pendiente)
            }).OrderByDescending(x => x.Cantidad),
            TopErrores = items.Where(x => x.Estado == ZuluIA_Back.Domain.Enums.EstadoImpresionSpool.Error && !string.IsNullOrWhiteSpace(x.MensajeError))
                .GroupBy(x => x.MensajeError)
                .Select(g => new { Mensaje = g.Key, Cantidad = g.Count() })
                .OrderByDescending(x => x.Cantidad)
                .Take(10)
        });
    }

    [HttpGet("fiscal/diagnostico")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDiagnosticoFiscal(CancellationToken ct)
        => Ok(await fiscalHardwareDiagnosticService.DiagnoseAsync(ct));

    [HttpGet("iva/{tipo}/pdf")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPdfLibroIva(string tipo, [FromQuery] long sucursalId, [FromQuery] DateOnly desde, [FromQuery] DateOnly hasta, CancellationToken ct)
    {
        if (!Enum.TryParse<TipoLibroIva>(tipo, true, out var tipoLibro))
            return BadRequest(new { error = "Tipo de libro IVA inválido. Use 'ventas' o 'compras'." });

        var pdf = await impresionDocumentosService.GenerarPdfLibroIvaAsync(sucursalId, desde, hasta, tipoLibro, ct);
        return File(pdf.Contenido, pdf.ContentType, pdf.NombreArchivo);
    }

    [HttpGet("reimpresiones/{categoria}/pdf")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPdfReimpresion(string categoria, [FromQuery] long? sucursalId, [FromQuery] long? ejercicioId, [FromQuery] DateOnly desde, [FromQuery] DateOnly hasta, [FromQuery] long? depositoId, CancellationToken ct)
    {
        if (!Enum.TryParse<CategoriaReimpresionReporte>(categoria, true, out var categoriaEnum))
            return BadRequest(new { error = "Categoría inválida. Use ventas, compras, stock o financieros." });

        try
        {
            var pdf = await impresionDocumentosService.GenerarPdfReimpresionAsync(categoriaEnum, sucursalId, ejercicioId, desde, hasta, depositoId, ct);
            return File(pdf.Contenido, pdf.ContentType, pdf.NombreArchivo);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}

public record ImprimirFiscalRequest(MarcaImpresoraFiscal Marca);
