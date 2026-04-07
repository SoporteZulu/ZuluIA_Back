using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ZuluIA_Back.Application.Common.Extensions;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Reportes.Enums;
using ZuluIA_Back.Application.Features.Reportes.Services;
using ZuluIA_Back.Application.Features.RRHH.Commands;
using ZuluIA_Back.Application.Features.RRHH.DTOs;
using ZuluIA_Back.Application.Features.RRHH.Services;
using ZuluIA_Back.Domain.Enums;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Api.Controllers;

public class EmpleadosController(
    IMediator mediator,
    IEmpleadoRepository repo,
    IApplicationDbContext db,
    IServiceProvider serviceProvider)
    : BaseController(mediator)
{
    private RrhhService rrhhService => serviceProvider.GetRequiredService<RrhhService>();
    private ReporteExportacionService reporteExportacionService => serviceProvider.GetRequiredService<ReporteExportacionService>();

    /// <summary>
    /// Retorna empleados paginados con filtros opcionales.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] long? sucursalId = null,
        [FromQuery] string? estado = null,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        EstadoEmpleado? estadoEnum = null;
        if (!string.IsNullOrWhiteSpace(estado) &&
            Enum.TryParse<EstadoEmpleado>(estado, true, out var parsed))
            estadoEnum = parsed;

        var result = await repo.GetPagedAsync(
            page, pageSize, sucursalId, estadoEnum, search, ct);

        var terceroIds = result.Items.Select(x => x.TerceroId).Distinct().ToList();
        var monedaIds = result.Items.Select(x => x.MonedaId).Distinct().ToList();

        var terceros = await db.Terceros.AsNoTracking()
            .Where(x => terceroIds.Contains(x.Id))
            .Select(x => new { x.Id, x.RazonSocial, x.NroDocumento })
            .ToDictionaryAsync(x => x.Id, ct);

        var monedas = await db.Monedas.AsNoTracking()
            .Where(x => monedaIds.Contains(x.Id))
            .Select(x => new { x.Id, x.Simbolo })
            .ToDictionaryAsync(x => x.Id, ct);

        var dtos = result.Items.Select(e => new EmpleadoDto
        {
            Id                 = e.Id,
            TerceroId          = e.TerceroId,
            TerceroRazonSocial = terceros.GetValueOrDefault(e.TerceroId)?.RazonSocial   ?? "—",
            TerceroCuit        = terceros.GetValueOrDefault(e.TerceroId)?.NroDocumento  ?? "—",
            SucursalId         = e.SucursalId,
            Legajo             = e.Legajo,
            Cargo              = e.Cargo,
            Area               = e.Area,
            FechaIngreso       = e.FechaIngreso,
            FechaEgreso        = e.FechaEgreso,
            SueldoBasico       = e.SueldoBasico,
            MonedaId           = e.MonedaId,
            MonedaSimbolo      = monedas.GetValueOrDefault(e.MonedaId)?.Simbolo ?? "$",
            Estado             = e.Estado.ToString().ToUpperInvariant()
        }).ToList();

        return Ok(new
        {
            data = dtos,
            page = result.Page,
            pageSize = result.PageSize,
            totalCount = result.TotalCount,
            totalPages = result.TotalPages
        });
    }

    /// <summary>
    /// Retorna el detalle de un empleado con sus liquidaciones.
    /// </summary>
    [HttpGet("{id:long}", Name = "GetEmpleadoById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
    {
        var emp = await repo.GetByIdAsync(id, ct);
        if (emp is null)
            return NotFound(new { error = $"No se encontró el empleado con ID {id}." });

        var tercero = await db.Terceros.AsNoTrackingSafe()
            .Where(x => x.Id == emp.TerceroId)
            .Select(x => new { x.RazonSocial, x.NroDocumento })
            .FirstOrDefaultSafeAsync(ct);

        var moneda = await db.Monedas.AsNoTrackingSafe()
            .Where(x => x.Id == emp.MonedaId)
            .Select(x => new { x.Simbolo })
            .FirstOrDefaultSafeAsync(ct);

        var monedaSimbolo = moneda?.Simbolo ?? "$";
        var terceroRazonSocial = tercero?.RazonSocial ?? "—";
        var terceroCuit = tercero?.NroDocumento ?? "—";

        var liquidaciones = await db.LiquidacionesSueldo.AsNoTrackingSafe()
            .Where(x => x.EmpleadoId == id)
            .OrderByDescending(x => x.Anio)
            .ThenByDescending(x => x.Mes)
            .Take(12)
            .Select(x => new LiquidacionSueldoDto
            {
                Id              = x.Id,
                EmpleadoId      = x.EmpleadoId,
                EmpleadoLegajo  = emp.Legajo,
                EmpleadoNombre  = terceroRazonSocial,
                SucursalId      = x.SucursalId,
                Anio            = x.Anio,
                Mes             = x.Mes,
                Periodo         = $"{x.Anio}/{x.Mes:D2}",
                SueldoBasico    = x.SueldoBasico,
                TotalHaberes    = x.TotalHaberes,
                TotalDescuentos = x.TotalDescuentos,
                Neto            = x.Neto,
                MonedaId        = x.MonedaId,
                MonedaSimbolo   = monedaSimbolo,
                Pagada          = x.Pagada,
                ImporteImputado = x.ImporteImputado,
                SaldoPendiente  = x.Neto - x.ImporteImputado,
                FechaPago       = x.FechaPago,
                ComprobanteEmpleadoId = x.ComprobanteEmpleadoId,
                Observacion     = x.Observacion,
                CreatedAt       = x.CreatedAt
            })
            .ToListSafeAsync(ct);

        var empleado = new EmpleadoDto
        {
            Id                 = emp.Id,
            TerceroId          = emp.TerceroId,
            TerceroRazonSocial = terceroRazonSocial,
            TerceroCuit        = terceroCuit,
            SucursalId         = emp.SucursalId,
            Legajo             = emp.Legajo,
            Cargo              = emp.Cargo,
            Area               = emp.Area,
            FechaIngreso       = emp.FechaIngreso,
            FechaEgreso        = emp.FechaEgreso,
            SueldoBasico       = emp.SueldoBasico,
            MonedaId           = emp.MonedaId,
            MonedaSimbolo      = monedaSimbolo,
            Estado             = emp.Estado.ToString().ToUpperInvariant()
        };

        return Ok(new
        {
            empleado.Id,
            empleado.TerceroId,
            empleado.TerceroRazonSocial,
            empleado.TerceroCuit,
            empleado.SucursalId,
            empleado.Legajo,
            empleado.Cargo,
            empleado.Area,
            empleado.FechaIngreso,
            empleado.FechaEgreso,
            empleado.SueldoBasico,
            empleado.MonedaId,
            empleado.MonedaSimbolo,
            empleado.Estado,
            Liquidaciones = liquidaciones
        });
    }

    /// <summary>
    /// Da de alta un nuevo empleado.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateEmpleadoCommand command,
        CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return CreatedFromResult(result, "GetEmpleadoById",
            new { id = result.IsSuccess ? result.Value : 0 });
    }

    [HttpPut("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateEmpleadoCommand command, CancellationToken ct)
    {
        if (id != command.Id)
            return BadRequest(new { error = "El ID de la URL no coincide con el del body." });

        return FromResult(await Mediator.Send(command, ct));
    }

    /// <summary>
    /// Registra el egreso de un empleado.
    /// </summary>
    [HttpPost("{id:long}/egresar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Egresar(
        long id,
        [FromBody] EgresarEmpleadoRequest request,
        CancellationToken ct)
    {
        var result = await Mediator.Send(new CambiarEstadoEmpleadoCommand(id, EstadoEmpleado.Inactivo, request.FechaEgreso), ct);
        if (result is null)
            return BadRequest(new { error = "No se pudo procesar el egreso del empleado." });

        if (result.IsFailure)
        {
            var error = result.Error?.Replace("encontro", "encontró", StringComparison.OrdinalIgnoreCase) ?? "No se pudo registrar el egreso del empleado.";
            return error.Contains("No se encontró", StringComparison.OrdinalIgnoreCase)
                ? NotFound(new { error })
                : BadRequest(new { error });
        }

        return Ok(new { message = "Egreso registrado correctamente." });
    }

    [HttpPost("{id:long}/suspender")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Suspender(long id, CancellationToken ct)
        => FromResult(await Mediator.Send(new CambiarEstadoEmpleadoCommand(id, EstadoEmpleado.Suspendido), ct));

    [HttpPost("{id:long}/reactivar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Reactivar(long id, CancellationToken ct)
        => FromResult(await Mediator.Send(new CambiarEstadoEmpleadoCommand(id, EstadoEmpleado.Activo), ct));

    /// <summary>
    /// Crea una liquidación de sueldo para un empleado.
    /// </summary>
    [HttpPost("{id:long}/liquidaciones")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateLiquidacion(
        long id,
        [FromBody] CreateLiquidacionSueldoCommand command,
        CancellationToken ct)
    {
        if (command.EmpleadoId != id)
            return BadRequest(new { error = "El ID del empleado no coincide." });

        var result = await Mediator.Send(command, ct);
        return result.IsSuccess
            ? Ok(new { id = result.Value })
            : BadRequest(new { error = result.Error });
    }

    [HttpGet("liquidaciones")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLiquidaciones([FromQuery] long? empleadoId = null, [FromQuery] bool? pagada = null, [FromQuery] int? anio = null, [FromQuery] int? mes = null, CancellationToken ct = default)
    {
        var query = db.LiquidacionesSueldo.AsNoTracking();
        if (empleadoId.HasValue)
            query = query.Where(x => x.EmpleadoId == empleadoId.Value);
        if (pagada.HasValue)
            query = query.Where(x => x.Pagada == pagada.Value);
        if (anio.HasValue)
            query = query.Where(x => x.Anio == anio.Value);
        if (mes.HasValue)
            query = query.Where(x => x.Mes == mes.Value);

        var items = await query.OrderByDescending(x => x.Anio).ThenByDescending(x => x.Mes).ThenByDescending(x => x.Id).ToListAsync(ct);
        var empleadoIds = items.Select(i => i.EmpleadoId).Distinct().ToList();
        var empleados = await db.Empleados.AsNoTracking().Where(x => empleadoIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, ct);
        var terceroIds = empleados.Values.Select(x => x.TerceroId).Distinct().ToList();
        var terceros = await db.Terceros.AsNoTracking().Where(x => terceroIds.Contains(x.Id))
            .Select(x => new { x.Id, x.RazonSocial })
            .ToDictionaryAsync(x => x.Id, ct);
        var monedaIds = items.Select(x => x.MonedaId).Distinct().ToList();
        var monedas = await db.Monedas.AsNoTracking().Where(x => monedaIds.Contains(x.Id))
            .Select(x => new { x.Id, x.Simbolo })
            .ToDictionaryAsync(x => x.Id, ct);

        return Ok(items.Select(x => new LiquidacionSueldoDto
        {
            Id = x.Id,
            EmpleadoId = x.EmpleadoId,
            EmpleadoLegajo = empleados.GetValueOrDefault(x.EmpleadoId)?.Legajo ?? "—",
            EmpleadoNombre = terceros.GetValueOrDefault(empleados.GetValueOrDefault(x.EmpleadoId)?.TerceroId ?? 0)?.RazonSocial ?? "—",
            SucursalId = x.SucursalId,
            Anio = x.Anio,
            Mes = x.Mes,
            Periodo = $"{x.Anio}/{x.Mes:D2}",
            SueldoBasico = x.SueldoBasico,
            TotalHaberes = x.TotalHaberes,
            TotalDescuentos = x.TotalDescuentos,
            Neto = x.Neto,
            MonedaId = x.MonedaId,
            MonedaSimbolo = monedas.GetValueOrDefault(x.MonedaId)?.Simbolo ?? "$",
            Pagada = x.Pagada,
            ImporteImputado = x.ImporteImputado,
            SaldoPendiente = x.SaldoPendiente,
            FechaPago = x.FechaPago,
            ComprobanteEmpleadoId = x.ComprobanteEmpleadoId,
            Observacion = x.Observacion,
            CreatedAt = x.CreatedAt
        }));
    }

    [HttpGet("liquidaciones/{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetLiquidacionById(long id, CancellationToken ct)
    {
        var liquidacion = await db.LiquidacionesSueldo.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        if (liquidacion is null)
            return NotFound(new { error = $"No se encontró la liquidación ID {id}." });

        var empleado = await db.Empleados.AsNoTracking().FirstOrDefaultAsync(x => x.Id == liquidacion.EmpleadoId, ct);
        var tercero = empleado is null
            ? null
            : await db.Terceros.AsNoTracking().Where(x => x.Id == empleado.TerceroId).Select(x => new { x.RazonSocial }).FirstOrDefaultAsync(ct);
        var moneda = await db.Monedas.AsNoTracking().Where(x => x.Id == liquidacion.MonedaId).Select(x => new { x.Simbolo }).FirstOrDefaultAsync(ct);

        return Ok(new LiquidacionSueldoDto
        {
            Id = liquidacion.Id,
            EmpleadoId = liquidacion.EmpleadoId,
            EmpleadoLegajo = empleado?.Legajo ?? "—",
            EmpleadoNombre = tercero?.RazonSocial ?? "—",
            SucursalId = liquidacion.SucursalId,
            Anio = liquidacion.Anio,
            Mes = liquidacion.Mes,
            Periodo = $"{liquidacion.Anio}/{liquidacion.Mes:D2}",
            SueldoBasico = liquidacion.SueldoBasico,
            TotalHaberes = liquidacion.TotalHaberes,
            TotalDescuentos = liquidacion.TotalDescuentos,
            Neto = liquidacion.Neto,
            MonedaId = liquidacion.MonedaId,
            MonedaSimbolo = moneda?.Simbolo ?? "$",
            Pagada = liquidacion.Pagada,
            ImporteImputado = liquidacion.ImporteImputado,
            SaldoPendiente = liquidacion.SaldoPendiente,
            FechaPago = liquidacion.FechaPago,
            ComprobanteEmpleadoId = liquidacion.ComprobanteEmpleadoId,
            Observacion = liquidacion.Observacion,
            CreatedAt = liquidacion.CreatedAt
        });
    }

    [HttpPut("liquidaciones/{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateLiquidacion(long id, [FromBody] UpdateLiquidacionSueldoCommand command, CancellationToken ct)
    {
        if (id != command.Id)
            return BadRequest(new { error = "El ID de la URL no coincide con el del body." });

        return FromResult(await Mediator.Send(command, ct));
    }

    [HttpPost("liquidaciones/masivas")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GenerarLiquidacionesMasivas([FromBody] GenerarLiquidacionesMasivasCommand command, CancellationToken ct)
        => FromResult(await Mediator.Send(command, ct));

    [HttpPost("liquidaciones/{id:long}/comprobante")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GenerarComprobante(long id, [FromBody] GenerarComprobanteEmpleadoRequest request, CancellationToken ct)
        => FromResult(await Mediator.Send(new GenerarComprobanteEmpleadoCommand(id, request.Fecha, request.Tipo, request.Observacion), ct));

    [HttpPost("liquidaciones/{id:long}/imputar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ImputarLiquidacion(long id, [FromBody] ImputarLiquidacionEmpleadoRequest request, CancellationToken ct)
        => FromResult(await Mediator.Send(new ImputarLiquidacionEmpleadoCommand(id, request.CajaId, request.Fecha, request.Importe, request.Observacion, request.GenerarComprobanteSiNoExiste), ct));

    [HttpGet("liquidaciones/{id:long}/imputaciones")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetImputaciones(long id, CancellationToken ct)
    {
        var imputaciones = await db.ImputacionesEmpleados.AsNoTracking()
            .Where(x => x.LiquidacionSueldoId == id && !x.IsDeleted)
            .OrderByDescending(x => x.Fecha)
            .ThenByDescending(x => x.Id)
            .ToListAsync(ct);

        return Ok(imputaciones.Select(x => new ImputacionEmpleadoDto
        {
            Id = x.Id,
            LiquidacionSueldoId = x.LiquidacionSueldoId,
            ComprobanteEmpleadoId = x.ComprobanteEmpleadoId,
            TesoreriaMovimientoId = x.TesoreriaMovimientoId,
            Fecha = x.Fecha,
            Importe = x.Importe,
            Observacion = x.Observacion,
            CreatedAt = x.CreatedAt
        }));
    }

    [HttpGet("imputaciones")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetImputacionesAll([FromQuery] long? empleadoId = null, [FromQuery] long? liquidacionSueldoId = null, CancellationToken ct = default)
    {
        var query = from imputacion in db.ImputacionesEmpleados.AsNoTracking().Where(x => !x.IsDeleted)
                    join liquidacion in db.LiquidacionesSueldo.AsNoTracking() on imputacion.LiquidacionSueldoId equals liquidacion.Id
                    select new { imputacion, liquidacion };

        if (empleadoId.HasValue)
            query = query.Where(x => x.liquidacion.EmpleadoId == empleadoId.Value);
        if (liquidacionSueldoId.HasValue)
            query = query.Where(x => x.imputacion.LiquidacionSueldoId == liquidacionSueldoId.Value);

        var items = await query.OrderByDescending(x => x.imputacion.Fecha).ThenByDescending(x => x.imputacion.Id).ToListAsync(ct);
        return Ok(items.Select(x => new ImputacionEmpleadoDto
        {
            Id = x.imputacion.Id,
            LiquidacionSueldoId = x.imputacion.LiquidacionSueldoId,
            ComprobanteEmpleadoId = x.imputacion.ComprobanteEmpleadoId,
            TesoreriaMovimientoId = x.imputacion.TesoreriaMovimientoId,
            Fecha = x.imputacion.Fecha,
            Importe = x.imputacion.Importe,
            Observacion = x.imputacion.Observacion,
            CreatedAt = x.imputacion.CreatedAt
        }));
    }

    [HttpGet("comprobantes")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetComprobantes([FromQuery] long? empleadoId = null, [FromQuery] long? liquidacionSueldoId = null, [FromQuery] string? tipo = null, CancellationToken ct = default)
    {
        var monedas = await db.Monedas.AsNoTracking().ToDictionaryAsync(x => x.Id, ct);
        var query = db.ComprobantesEmpleados.AsNoTracking().Where(x => !x.IsDeleted);
        if (empleadoId.HasValue)
            query = query.Where(x => x.EmpleadoId == empleadoId.Value);
        if (liquidacionSueldoId.HasValue)
            query = query.Where(x => x.LiquidacionSueldoId == liquidacionSueldoId.Value);
        if (!string.IsNullOrWhiteSpace(tipo))
        {
            var tipoNormalizado = tipo.Trim().ToUpperInvariant();
            query = query.Where(x => x.Tipo == tipoNormalizado);
        }

        var items = await query.OrderByDescending(x => x.Fecha).ThenByDescending(x => x.Id).ToListAsync(ct);
        return Ok(items.Select(x => new ComprobanteEmpleadoDto
        {
            Id = x.Id,
            EmpleadoId = x.EmpleadoId,
            LiquidacionSueldoId = x.LiquidacionSueldoId,
            SucursalId = x.SucursalId,
            Fecha = x.Fecha,
            Tipo = x.Tipo,
            Numero = x.Numero,
            Total = x.Total,
            MonedaId = x.MonedaId,
            MonedaSimbolo = monedas.GetValueOrDefault(x.MonedaId)?.Simbolo ?? "$",
            Observacion = x.Observacion
        }));
    }

    [HttpGet("comprobantes/{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetComprobanteById(long id, CancellationToken ct)
    {
        var item = await db.ComprobantesEmpleados.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
        if (item is null)
            return NotFound(new { error = $"No se encontró el comprobante de empleado ID {id}." });

        var moneda = await db.Monedas.AsNoTracking().Where(x => x.Id == item.MonedaId).Select(x => new { x.Simbolo }).FirstOrDefaultAsync(ct);
        return Ok(new ComprobanteEmpleadoDto
        {
            Id = item.Id,
            EmpleadoId = item.EmpleadoId,
            LiquidacionSueldoId = item.LiquidacionSueldoId,
            SucursalId = item.SucursalId,
            Fecha = item.Fecha,
            Tipo = item.Tipo,
            Numero = item.Numero,
            Total = item.Total,
            MonedaId = item.MonedaId,
            MonedaSimbolo = moneda?.Simbolo ?? "$",
            Observacion = item.Observacion
        });
    }

    [HttpGet("comprobantes/{id:long}/pdf")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ReimprimirComprobante(long id, CancellationToken ct)
    {
        try
        {
            var pdf = await rrhhService.GenerarPdfComprobanteEmpleadoAsync(id, ct);
            return File(pdf.Contenido, pdf.ContentType, pdf.NombreArchivo);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpGet("resumen")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetResumen([FromQuery] long? sucursalId = null, CancellationToken ct = default)
    {
        var empleados = db.Empleados.AsNoTracking();
        var liquidaciones = db.LiquidacionesSueldo.AsNoTracking();
        if (sucursalId.HasValue)
        {
            empleados = empleados.Where(x => x.SucursalId == sucursalId.Value);
            liquidaciones = liquidaciones.Where(x => x.SucursalId == sucursalId.Value);
        }

        var empItems = await empleados.ToListAsync(ct);
        var liqItems = await liquidaciones.ToListAsync(ct);
        return Ok(new
        {
            Empleados = empItems.Count,
            Activos = empItems.Count(x => x.Estado == EstadoEmpleado.Activo),
            Suspendidos = empItems.Count(x => x.Estado == EstadoEmpleado.Suspendido),
            Inactivos = empItems.Count(x => x.Estado == EstadoEmpleado.Inactivo),
            Liquidaciones = liqItems.Count,
            Pagadas = liqItems.Count(x => x.Pagada),
            Pendientes = liqItems.Count(x => !x.Pagada),
            NetoLiquidado = liqItems.Sum(x => x.Neto),
            SaldoPendiente = liqItems.Sum(x => x.SaldoPendiente)
        });
    }

    [HttpGet("reportes/empleados")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportarReporteEmpleados([FromQuery] long? sucursalId = null, [FromQuery] string? estado = null, [FromQuery] FormatoExportacionReporte formato = FormatoExportacionReporte.Csv, CancellationToken ct = default)
    {
        EstadoEmpleado? estadoEnum = null;
        if (!string.IsNullOrWhiteSpace(estado) && Enum.TryParse<EstadoEmpleado>(estado, true, out var parsed))
            estadoEnum = parsed;

        var reporte = await rrhhService.GetReporteEmpleadosAsync(sucursalId, estadoEnum, ct);
        var archivo = reporteExportacionService.Exportar(reporte, formato, "rrhh_empleados");
        return File(archivo.Contenido, archivo.ContentType, archivo.NombreArchivo);
    }

    [HttpGet("reportes/liquidaciones")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportarReporteLiquidaciones([FromQuery] long? empleadoId = null, [FromQuery] bool? pagada = null, [FromQuery] int? anio = null, [FromQuery] int? mes = null, [FromQuery] FormatoExportacionReporte formato = FormatoExportacionReporte.Csv, CancellationToken ct = default)
    {
        var reporte = await rrhhService.GetReporteLiquidacionesAsync(empleadoId, pagada, anio, mes, ct);
        var archivo = reporteExportacionService.Exportar(reporte, formato, "rrhh_liquidaciones");
        return File(archivo.Contenido, archivo.ContentType, archivo.NombreArchivo);
    }

    [HttpGet("reportes/comprobantes")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportarReporteComprobantes([FromQuery] long? empleadoId = null, [FromQuery] long? liquidacionSueldoId = null, [FromQuery] string? tipo = null, [FromQuery] FormatoExportacionReporte formato = FormatoExportacionReporte.Csv, CancellationToken ct = default)
    {
        var reporte = await rrhhService.GetReporteComprobantesAsync(empleadoId, liquidacionSueldoId, tipo, ct);
        var archivo = reporteExportacionService.Exportar(reporte, formato, "rrhh_comprobantes");
        return File(archivo.Contenido, archivo.ContentType, archivo.NombreArchivo);
    }

    [HttpGet("reportes/imputaciones")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportarReporteImputaciones([FromQuery] long? empleadoId = null, [FromQuery] long? liquidacionSueldoId = null, [FromQuery] FormatoExportacionReporte formato = FormatoExportacionReporte.Csv, CancellationToken ct = default)
    {
        var reporte = await rrhhService.GetReporteImputacionesAsync(empleadoId, liquidacionSueldoId, ct);
        var archivo = reporteExportacionService.Exportar(reporte, formato, "rrhh_imputaciones");
        return File(archivo.Contenido, archivo.ContentType, archivo.NombreArchivo);
    }
}

public record EgresarEmpleadoRequest(DateOnly FechaEgreso);
public record GenerarComprobanteEmpleadoRequest(DateOnly Fecha, string Tipo = "RECIBO_SUELDO", string? Observacion = null);
public record ImputarLiquidacionEmpleadoRequest(long CajaId, DateOnly Fecha, decimal Importe, string? Observacion, bool GenerarComprobanteSiNoExiste = true);
