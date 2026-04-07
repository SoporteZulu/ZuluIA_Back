using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Colegio.Commands;
using ZuluIA_Back.Application.Features.Colegio.Services;
using ZuluIA_Back.Application.Features.Reportes.Enums;
using ZuluIA_Back.Application.Features.Reportes.Services;

namespace ZuluIA_Back.Api.Controllers;

public class ColegioController(IMediator mediator, IApplicationDbContext db, IServiceProvider serviceProvider) : BaseController(mediator)
{
    private ColegioService colegioService => serviceProvider.GetRequiredService<ColegioService>();
    private ReporteExportacionService reporteExportacionService => serviceProvider.GetRequiredService<ReporteExportacionService>();

    [HttpGet("planes-generales")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPlanesGenerales([FromQuery] long? sucursalId = null, CancellationToken ct = default)
    {
        var query = db.ColegioPlanesGenerales.AsNoTracking().Where(x => !x.IsDeleted && x.Activo);
        if (sucursalId.HasValue)
            query = query.Where(x => x.SucursalId == sucursalId.Value);

        return Ok(await query.OrderBy(x => x.Codigo).ToListAsync(ct));
    }

    [HttpPost("planes-generales")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> CreatePlanGeneral([FromBody] CreatePlanGeneralColegioCommand command, CancellationToken ct)
        => FromResult(await Mediator.Send(command, ct));

    [HttpPut("planes-generales/{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdatePlanGeneral(long id, [FromBody] UpdatePlanGeneralColegioCommand command, CancellationToken ct)
    {
        if (id != command.Id)
            return BadRequest(new { error = "El ID de la URL no coincide con el del body." });

        return FromResult(await Mediator.Send(command, ct));
    }

    [HttpPost("lotes/{id:long}/cerrar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CerrarLote(long id, [FromBody] CerrarLoteColegioRequest request, CancellationToken ct)
        => FromResult(await Mediator.Send(new CerrarLoteColegioCommand(id, request.Observacion), ct));

    [HttpDelete("planes-generales/{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeletePlanGeneral(long id, CancellationToken ct)
        => FromResult(await Mediator.Send(new DesactivarPlanGeneralColegioCommand(id), ct));

    [HttpGet("planes-generales/{id:long}/detalle")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPlanGeneralDetalle(long id, CancellationToken ct)
    {
        var plan = await db.ColegioPlanesGenerales.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
        if (plan is null)
            return NotFound(new { error = $"No se encontró el plan general de colegio ID {id}." });

        var lotes = await db.ColegioLotes.AsNoTracking().Where(x => x.PlanGeneralColegioId == id && !x.IsDeleted).ToListAsync(ct);
        var loteIds = lotes.Select(x => x.Id).ToList();
        var cedulones = loteIds.Count == 0
            ? []
            : await db.Cedulones.AsNoTracking().Where(x => x.PlanGeneralColegioId == id && !x.IsDeleted).ToListAsync(ct);

        return Ok(new
        {
            plan.Id,
            plan.SucursalId,
            plan.PlanPagoId,
            plan.TipoComprobanteId,
            plan.ItemId,
            plan.MonedaId,
            plan.Codigo,
            plan.Descripcion,
            plan.ImporteBase,
            plan.Activo,
            plan.Observacion,
            CantidadLotes = lotes.Count,
            CantidadCedulones = cedulones.Count,
            ImporteEmitido = cedulones.Sum(x => x.Importe),
            ImportePagado = cedulones.Sum(x => x.ImportePagado),
            SaldoPendiente = cedulones.Sum(x => x.Importe - x.ImportePagado)
        });
    }

    [HttpGet("lotes")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLotes([FromQuery] long? planGeneralColegioId = null, CancellationToken ct = default)
    {
        var query = db.ColegioLotes.AsNoTracking().Where(x => !x.IsDeleted);
        if (planGeneralColegioId.HasValue)
            query = query.Where(x => x.PlanGeneralColegioId == planGeneralColegioId.Value);

        return Ok(await query.OrderByDescending(x => x.FechaEmision).ThenByDescending(x => x.Id).ToListAsync(ct));
    }

    [HttpPost("lotes")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateLote([FromBody] CreateLoteColegioCommand command, CancellationToken ct)
        => FromResult(await Mediator.Send(command, ct));

    [HttpPut("lotes/{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateLote(long id, [FromBody] UpdateLoteColegioCommand command, CancellationToken ct)
    {
        if (id != command.Id)
            return BadRequest(new { error = "El ID de la URL no coincide con el del body." });

        return FromResult(await Mediator.Send(command, ct));
    }

    [HttpGet("lotes/{id:long}/detalle")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetLoteDetalle(long id, CancellationToken ct)
    {
        var lote = await db.ColegioLotes.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
        if (lote is null)
            return NotFound(new { error = $"No se encontró el lote de colegio ID {id}." });

        var cedulones = await db.Cedulones.AsNoTracking().Where(x => x.LoteColegioId == id && !x.IsDeleted).ToListAsync(ct);
        return Ok(new
        {
            lote.Id,
            lote.PlanGeneralColegioId,
            lote.Codigo,
            lote.FechaEmision,
            lote.FechaVencimiento,
            Estado = lote.Estado.ToString().ToUpperInvariant(),
            lote.CantidadCedulones,
            lote.Observacion,
            Resumen = new
            {
                TotalCedulones = cedulones.Count,
                Pendientes = cedulones.Count(x => x.Estado == Domain.Enums.EstadoCedulon.Pendiente || x.Estado == Domain.Enums.EstadoCedulon.Vencido),
                PagadosParcial = cedulones.Count(x => x.Estado == Domain.Enums.EstadoCedulon.PagadoParcial),
                Pagados = cedulones.Count(x => x.Estado == Domain.Enums.EstadoCedulon.Pagado),
                Facturados = cedulones.Count(x => x.ComprobanteId.HasValue),
                ImporteEmitido = cedulones.Sum(x => x.Importe),
                ImportePagado = cedulones.Sum(x => x.ImportePagado),
                SaldoPendiente = cedulones.Sum(x => x.Importe - x.ImportePagado)
            }
        });
    }

    [HttpPost("lotes/{id:long}/emitir-cedulones")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> EmitirCedulones(long id, [FromBody] EmitirCedulonesColegioRequest request, CancellationToken ct)
        => FromResult(await Mediator.Send(new EmitirCedulonesColegioCommand(id, request.TerceroIds), ct));

    [HttpGet("cedulones")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCedulones([FromQuery] long? loteId = null, [FromQuery] long? planGeneralColegioId = null, [FromQuery] long? terceroId = null, [FromQuery] bool soloPendientes = false, CancellationToken ct = default)
    {
        var query = db.Cedulones.AsNoTracking().Where(x => !x.IsDeleted && x.PlanGeneralColegioId.HasValue);
        if (loteId.HasValue)
            query = query.Where(x => x.LoteColegioId == loteId.Value);
        if (planGeneralColegioId.HasValue)
            query = query.Where(x => x.PlanGeneralColegioId == planGeneralColegioId.Value);
        if (terceroId.HasValue)
            query = query.Where(x => x.TerceroId == terceroId.Value);
        if (soloPendientes)
            query = query.Where(x => x.ImportePagado < x.Importe);

        var items = await query.OrderByDescending(x => x.FechaEmision).ThenByDescending(x => x.Id).ToListAsync(ct);
        return Ok(items.Select(x => new
        {
            x.Id,
            x.TerceroId,
            x.SucursalId,
            x.PlanPagoId,
            x.PlanGeneralColegioId,
            x.LoteColegioId,
            x.ComprobanteId,
            x.NroCedulon,
            x.FechaEmision,
            x.FechaVencimiento,
            x.Importe,
            x.ImportePagado,
            SaldoPendiente = x.Importe - x.ImportePagado,
            Estado = x.Estado.ToString().ToUpperInvariant()
        }));
    }

    [HttpPost("cancelaciones-deuda")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> CancelarDeuda([FromBody] CancelarDeudaColegioCommand command, CancellationToken ct)
        => FromResult(await Mediator.Send(command, ct));

    [HttpPost("cancelaciones-deuda/masiva")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> CancelarDeudaMasiva([FromBody] CancelarDeudaColegioMasivaCommand command, CancellationToken ct)
        => FromResult(await Mediator.Send(command, ct));

    [HttpPost("recibos/masivos")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> RegistrarRecibosMasivos([FromBody] CancelarDeudaColegioMasivaCommand command, CancellationToken ct)
        => FromResult(await Mediator.Send(command, ct));

    [HttpGet("recibos")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRecibos([FromQuery] long? terceroId = null, [FromQuery] DateOnly? desde = null, [FromQuery] DateOnly? hasta = null, CancellationToken ct = default)
    {
        var query = db.Cobros.AsNoTracking().Where(x => !x.IsDeleted && x.Observacion != null && x.Observacion.Contains("COLEGIO"));
        if (terceroId.HasValue)
            query = query.Where(x => x.TerceroId == terceroId.Value);
        if (desde.HasValue)
            query = query.Where(x => x.Fecha >= desde.Value);
        if (hasta.HasValue)
            query = query.Where(x => x.Fecha <= hasta.Value);

        var cobros = await query.OrderByDescending(x => x.Fecha).ThenByDescending(x => x.Id).ToListAsync(ct);
        var detalles = await db.ColegioRecibosDetalles.AsNoTracking().Where(x => cobros.Select(c => c.Id).Contains(x.CobroId)).ToListAsync(ct);
        return Ok(cobros.Select(x => new
        {
            x.Id,
            x.SucursalId,
            x.TerceroId,
            x.Fecha,
            x.Total,
            x.Observacion,
                CantidadCedulones = detalles.Count(d => d.CobroId == x.Id),
            Detalles = detalles.Where(d => d.CobroId == x.Id).Select(d => new { d.CedulonId, d.Importe })
        }));
    }

    [HttpGet("recibos/{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetReciboDetalle(long id, CancellationToken ct)
    {
        var cobro = await db.Cobros.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted && x.Observacion != null && x.Observacion.Contains("COLEGIO"), ct);
        if (cobro is null)
            return NotFound(new { error = $"No se encontró el recibo de colegio ID {id}." });

        var detalles = await db.ColegioRecibosDetalles.AsNoTracking().Where(x => x.CobroId == id).ToListAsync(ct);
        var cedulones = await db.Cedulones.AsNoTracking().Where(x => detalles.Select(d => d.CedulonId).Contains(x.Id)).ToDictionaryAsync(x => x.Id, ct);

        return Ok(new
        {
            cobro.Id,
            cobro.SucursalId,
            cobro.TerceroId,
            cobro.Fecha,
            cobro.Total,
            cobro.Observacion,
            Detalles = detalles.Select(d => new
            {
                d.CedulonId,
                NroCedulon = cedulones.GetValueOrDefault(d.CedulonId)?.NroCedulon,
                d.Importe
            })
        });
    }

    [HttpPost("cobinpro")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> RegistrarCobinpro([FromBody] RegistrarCobinproColegioCommand command, CancellationToken ct)
        => FromResult(await Mediator.Send(command, ct));

    [HttpPost("cobinpro/{id:long}/conciliar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ConciliarCobinpro(long id, [FromBody] ConciliarCobinproColegioRequest request, CancellationToken ct)
        => FromResult(await Mediator.Send(new ConciliarCobinproColegioCommand(id, request.Confirmar, request.Observacion), ct));

    [HttpGet("cobinpro")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCobinpro(CancellationToken ct)
        => Ok(await db.ColegioCobinproOperaciones.AsNoTracking().OrderByDescending(x => x.Fecha).ThenByDescending(x => x.Id).ToListAsync(ct));

    [HttpPost("facturacion")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Facturar([FromBody] FacturarCedulonesColegioCommand command, CancellationToken ct)
        => FromResult(await Mediator.Send(command, ct));

    [HttpPost("facturacion/automatica")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> FacturarAutomatica([FromBody] FacturarCedulonesColegioAutomaticoCommand command, CancellationToken ct)
        => FromResult(await Mediator.Send(command, ct));

    [HttpPost("cedulones/vencer")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VencerCedulones([FromBody] VencerCedulonesColegioCommand command, CancellationToken ct)
        => FromResult(await Mediator.Send(command, ct));

    [HttpGet("deudas-pendientes")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDeudasPendientes([FromQuery] long? sucursalId = null, [FromQuery] long? terceroId = null, [FromQuery] long? loteId = null, [FromQuery] DateOnly? hastaVencimiento = null, CancellationToken ct = default)
    {
        var query = db.Cedulones.AsNoTracking().Where(x => !x.IsDeleted && x.PlanGeneralColegioId.HasValue && x.ImportePagado < x.Importe);
        if (sucursalId.HasValue)
            query = query.Where(x => x.SucursalId == sucursalId.Value);
        if (terceroId.HasValue)
            query = query.Where(x => x.TerceroId == terceroId.Value);
        if (loteId.HasValue)
            query = query.Where(x => x.LoteColegioId == loteId.Value);
        if (hastaVencimiento.HasValue)
            query = query.Where(x => x.FechaVencimiento <= hastaVencimiento.Value);

        var items = await query.OrderBy(x => x.FechaVencimiento).ThenBy(x => x.Id).ToListAsync(ct);
        return Ok(items.Select(x => new
        {
            x.Id,
            x.TerceroId,
            x.SucursalId,
            x.PlanGeneralColegioId,
            x.LoteColegioId,
            x.NroCedulon,
            x.FechaEmision,
            x.FechaVencimiento,
            x.Importe,
            x.ImportePagado,
            SaldoPendiente = x.Importe - x.ImportePagado,
            Estado = x.Estado.ToString().ToUpperInvariant()
        }));
    }


    [HttpGet("resumen")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetResumen([FromQuery] long? sucursalId = null, [FromQuery] long? planGeneralColegioId = null, [FromQuery] long? loteId = null, CancellationToken ct = default)
    {
        var cedulones = db.Cedulones.AsNoTracking().Where(x => !x.IsDeleted && x.PlanGeneralColegioId.HasValue);
        if (sucursalId.HasValue)
            cedulones = cedulones.Where(x => x.SucursalId == sucursalId.Value);
        if (planGeneralColegioId.HasValue)
            cedulones = cedulones.Where(x => x.PlanGeneralColegioId == planGeneralColegioId.Value);
        if (loteId.HasValue)
            cedulones = cedulones.Where(x => x.LoteColegioId == loteId.Value);

        var cedulonesList = await cedulones.ToListAsync(ct);
        var cobrosIds = await db.ColegioRecibosDetalles.AsNoTracking().Select(x => x.CobroId).Distinct().ToListAsync(ct);
        var cobros = db.Cobros.AsNoTracking().Where(x => cobrosIds.Contains(x.Id) && !x.IsDeleted);
        if (sucursalId.HasValue)
            cobros = cobros.Where(x => x.SucursalId == sucursalId.Value);

        var cobrosList = await cobros.ToListAsync(ct);
        var cobinpro = db.ColegioCobinproOperaciones.AsNoTracking();
        if (sucursalId.HasValue)
            cobinpro = cobinpro.Where(x => x.SucursalId == sucursalId.Value);

        var cobinproList = await cobinpro.ToListAsync(ct);

        return Ok(new
        {
            CantidadCedulones = cedulonesList.Count,
            Pendientes = cedulonesList.Count(x => x.ImportePagado < x.Importe),
            Pagados = cedulonesList.Count(x => x.Estado == Domain.Enums.EstadoCedulon.Pagado),
            Vencidos = cedulonesList.Count(x => x.Estado == Domain.Enums.EstadoCedulon.Vencido),
            Facturados = cedulonesList.Count(x => x.ComprobanteId.HasValue),
            ImporteEmitido = cedulonesList.Sum(x => x.Importe),
            ImportePagado = cedulonesList.Sum(x => x.ImportePagado),
            SaldoPendiente = cedulonesList.Sum(x => x.Importe - x.ImportePagado),
            CantidadRecibos = cobrosList.Count,
            TotalRecibos = cobrosList.Sum(x => x.Total),
            CantidadCobinpro = cobinproList.Count,
            TotalCobinpro = cobinproList.Sum(x => x.Importe)
        });
    }

    [HttpGet("reportes/cedulones")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportarReporteCedulones([FromQuery] long? loteId, [FromQuery] long? planGeneralColegioId, [FromQuery] bool soloPendientes = false, [FromQuery] FormatoExportacionReporte formato = FormatoExportacionReporte.Csv, CancellationToken ct = default)
    {
        var reporte = await colegioService.GetReporteCedulonesAsync(loteId, planGeneralColegioId, soloPendientes, ct);
        var archivo = reporteExportacionService.Exportar(reporte, formato, "colegio_cedulones");
        return File(archivo.Contenido, archivo.ContentType, archivo.NombreArchivo);
    }

    [HttpGet("reportes/recibos")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportarReporteRecibos([FromQuery] long? terceroId, [FromQuery] DateOnly? desde, [FromQuery] DateOnly? hasta, [FromQuery] FormatoExportacionReporte formato = FormatoExportacionReporte.Csv, CancellationToken ct = default)
    {
        var reporte = await colegioService.GetReporteRecibosAsync(terceroId, desde, hasta, ct);
        var archivo = reporteExportacionService.Exportar(reporte, formato, "colegio_recibos");
        return File(archivo.Contenido, archivo.ContentType, archivo.NombreArchivo);
    }
}

public record EmitirCedulonesColegioRequest(IReadOnlyList<long> TerceroIds);
public record CerrarLoteColegioRequest(string? Observacion);
public record ConciliarCobinproColegioRequest(bool Confirmar, string? Observacion);
