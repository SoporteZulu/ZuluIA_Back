using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Contratos.Commands;
using ZuluIA_Back.Application.Features.Contratos.Services;
using ZuluIA_Back.Application.Features.Reportes.Enums;
using ZuluIA_Back.Application.Features.Reportes.Services;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Api.Controllers;

public class ContratosController(IMediator mediator, IApplicationDbContext db, IServiceProvider serviceProvider) : BaseController(mediator)
{
    private ContratosService contratosService => serviceProvider.GetRequiredService<ContratosService>();
    private ReporteExportacionService reporteExportacionService => serviceProvider.GetRequiredService<ReporteExportacionService>();

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] long? terceroId = null, [FromQuery] long? sucursalId = null, [FromQuery] string? estado = null, [FromQuery] string? codigo = null, [FromQuery] bool soloVigentes = false, CancellationToken ct = default)
    {
        EstadoContrato? estadoEnum = null;
        if (!string.IsNullOrWhiteSpace(estado) && Enum.TryParse<EstadoContrato>(estado, true, out var parsed))
            estadoEnum = parsed;

        var query = db.Contratos.AsNoTracking().Where(x => !x.IsDeleted);
        if (terceroId.HasValue)
            query = query.Where(x => x.TerceroId == terceroId.Value);
        if (sucursalId.HasValue)
            query = query.Where(x => x.SucursalId == sucursalId.Value);
        if (estadoEnum.HasValue)
            query = query.Where(x => x.Estado == estadoEnum.Value);
        if (!string.IsNullOrWhiteSpace(codigo))
            query = query.Where(x => x.Codigo.Contains(codigo.Trim().ToUpper()));
        if (soloVigentes)
        {
            var hoy = DateOnly.FromDateTime(DateTime.Today);
            query = query.Where(x => x.FechaInicio <= hoy && x.FechaFin >= hoy && x.Estado != EstadoContrato.Cancelado && x.Estado != EstadoContrato.Finalizado);
        }

        return Ok(await query.OrderByDescending(x => x.FechaInicio).ThenByDescending(x => x.Id).ToListAsync(ct));
    }

    [HttpGet("{id:long}", Name = "GetContratoById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
    {
        var contrato = await db.Contratos.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
        return OkOrNotFound(contrato);
    }

    [HttpGet("{id:long}/detalle")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDetalle(long id, CancellationToken ct)
    {
        var contrato = await db.Contratos.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
        if (contrato is null)
            return NotFound(new { error = $"No se encontró el contrato ID {id}." });

        var historial = await db.ContratosHistorial.AsNoTracking().Where(x => x.ContratoId == id && !x.IsDeleted).OrderByDescending(x => x.Fecha).ThenByDescending(x => x.Id).ToListAsync(ct);
        var impactos = await db.ContratosImpactos.AsNoTracking().Where(x => x.ContratoId == id && !x.IsDeleted).OrderByDescending(x => x.Fecha).ThenByDescending(x => x.Id).ToListAsync(ct);

        return Ok(new
        {
            contrato.Id,
            contrato.TerceroId,
            contrato.SucursalId,
            contrato.MonedaId,
            contrato.Codigo,
            contrato.Descripcion,
            contrato.FechaInicio,
            contrato.FechaFin,
            contrato.Importe,
            contrato.RenovacionAutomatica,
            Estado = contrato.Estado.ToString().ToUpperInvariant(),
            contrato.Observacion,
            Resumen = new
            {
                CantidadEventos = historial.Count,
                CantidadImpactos = impactos.Count,
                ImpactoComercial = impactos.Where(x => x.Tipo == TipoImpactoContrato.Comercial).Sum(x => x.Importe),
                ImpactoFinanciero = impactos.Where(x => x.Tipo == TipoImpactoContrato.Financiero).Sum(x => x.Importe)
            },
            Historial = historial,
            Impactos = impactos
        });
    }

    [HttpGet("{id:long}/historial")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHistorial(long id, CancellationToken ct)
        => Ok(await db.ContratosHistorial.AsNoTracking().Where(x => x.ContratoId == id && !x.IsDeleted).OrderByDescending(x => x.Fecha).ThenByDescending(x => x.Id).ToListAsync(ct));

    [HttpGet("historial")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHistorialGlobal([FromQuery] long? contratoId = null, [FromQuery] long? terceroId = null, [FromQuery] long? sucursalId = null, [FromQuery] string? tipoEvento = null, [FromQuery] DateOnly? desde = null, [FromQuery] DateOnly? hasta = null, CancellationToken ct = default)
    {
        TipoEventoContrato? tipoEventoEnum = null;
        if (!string.IsNullOrWhiteSpace(tipoEvento) && Enum.TryParse<TipoEventoContrato>(tipoEvento, true, out var parsed))
            tipoEventoEnum = parsed;

        var contratoIds = await db.Contratos.AsNoTracking()
            .Where(x => !x.IsDeleted
                && (!contratoId.HasValue || x.Id == contratoId.Value)
                && (!terceroId.HasValue || x.TerceroId == terceroId.Value)
                && (!sucursalId.HasValue || x.SucursalId == sucursalId.Value))
            .Select(x => x.Id)
            .ToListAsync(ct);

        var query = db.ContratosHistorial.AsNoTracking().Where(x => !x.IsDeleted && contratoIds.Contains(x.ContratoId));
        if (tipoEventoEnum.HasValue)
            query = query.Where(x => x.TipoEvento == tipoEventoEnum.Value);
        if (desde.HasValue)
            query = query.Where(x => x.Fecha >= desde.Value);
        if (hasta.HasValue)
            query = query.Where(x => x.Fecha <= hasta.Value);

        return Ok(await query.OrderByDescending(x => x.Fecha).ThenByDescending(x => x.Id).ToListAsync(ct));
    }

    [HttpGet("{id:long}/impactos")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetImpactos(long id, CancellationToken ct)
        => Ok(await db.ContratosImpactos.AsNoTracking().Where(x => x.ContratoId == id && !x.IsDeleted).OrderByDescending(x => x.Fecha).ThenByDescending(x => x.Id).ToListAsync(ct));

    [HttpGet("impactos")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetImpactosGlobal([FromQuery] long? contratoId = null, [FromQuery] long? terceroId = null, [FromQuery] long? sucursalId = null, [FromQuery] string? tipoImpacto = null, [FromQuery] DateOnly? desde = null, [FromQuery] DateOnly? hasta = null, CancellationToken ct = default)
    {
        TipoImpactoContrato? tipoImpactoEnum = null;
        if (!string.IsNullOrWhiteSpace(tipoImpacto) && Enum.TryParse<TipoImpactoContrato>(tipoImpacto, true, out var parsed))
            tipoImpactoEnum = parsed;

        var contratoIds = await db.Contratos.AsNoTracking()
            .Where(x => !x.IsDeleted
                && (!contratoId.HasValue || x.Id == contratoId.Value)
                && (!terceroId.HasValue || x.TerceroId == terceroId.Value)
                && (!sucursalId.HasValue || x.SucursalId == sucursalId.Value))
            .Select(x => x.Id)
            .ToListAsync(ct);

        var query = db.ContratosImpactos.AsNoTracking().Where(x => !x.IsDeleted && contratoIds.Contains(x.ContratoId));
        if (tipoImpactoEnum.HasValue)
            query = query.Where(x => x.Tipo == tipoImpactoEnum.Value);
        if (desde.HasValue)
            query = query.Where(x => x.Fecha >= desde.Value);
        if (hasta.HasValue)
            query = query.Where(x => x.Fecha <= hasta.Value);

        return Ok(await query.OrderByDescending(x => x.Fecha).ThenByDescending(x => x.Id).ToListAsync(ct));
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateContratoCommand command, CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return CreatedFromResult(result, "GetContratoById", new { id = result.IsSuccess ? result.Value : 0 });
    }

    [HttpPut("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateContratoCommand command, CancellationToken ct)
    {
        if (id != command.Id)
            return BadRequest(new { error = "El ID de la URL no coincide con el del body." });
        return FromResult(await Mediator.Send(command, ct));
    }

    [HttpPost("{id:long}/renovar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Renovar(long id, [FromBody] RenovarContratoRequest request, CancellationToken ct)
        => FromResult(await Mediator.Send(new RenovarContratoCommand(id, request.NuevaFechaFin, request.NuevoImporte, request.Observacion, request.GenerarImpactoComercial, request.GenerarImpactoFinanciero), ct));

    [HttpPost("{id:long}/cancelar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Cancelar(long id, [FromBody] CancelarContratoRequest request, CancellationToken ct)
        => FromResult(await Mediator.Send(new CancelarContratoCommand(id, request.Observacion), ct));

    [HttpPost("{id:long}/impactos")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegistrarImpacto(long id, [FromBody] RegistrarImpactoContratoRequest request, CancellationToken ct)
        => FromResult(await Mediator.Send(new RegistrarImpactoContratoCommand(id, request.Tipo, request.Fecha, request.Importe, request.Descripcion, request.ImpactarCuentaCorriente), ct));

    [HttpPost("finalizar-vencidos")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> FinalizarVencidos([FromBody] FinalizarContratosVencidosRequest request, CancellationToken ct)
        => FromResult(await Mediator.Send(new FinalizarContratosVencidosCommand(request.SucursalId, request.FechaCorte), ct));

    [HttpPost("renovar-automaticamente")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RenovarAutomaticamente([FromBody] RenovarContratosAutomaticamenteRequest request, CancellationToken ct)
        => FromResult(await Mediator.Send(new RenovarContratosAutomaticamenteCommand(request.SucursalId, request.FechaCorte, request.PorcentajeAjuste, request.GenerarImpactoComercial, request.GenerarImpactoFinanciero), ct));

    [HttpGet("resumen")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetResumen([FromQuery] long? terceroId = null, [FromQuery] long? sucursalId = null, CancellationToken ct = default)
    {
        var query = db.Contratos.AsNoTracking().Where(x => !x.IsDeleted);
        if (terceroId.HasValue)
            query = query.Where(x => x.TerceroId == terceroId.Value);
        if (sucursalId.HasValue)
            query = query.Where(x => x.SucursalId == sucursalId.Value);

        var contratos = await query.ToListAsync(ct);
        var contratoIds = contratos.Select(x => x.Id).ToList();
        var impactos = contratoIds.Count == 0
            ? new List<ZuluIA_Back.Domain.Entities.Contratos.ContratoImpacto>()
            : await db.ContratosImpactos.AsNoTracking().Where(x => contratoIds.Contains(x.ContratoId) && !x.IsDeleted).ToListAsync(ct);

        return Ok(new
        {
            Cantidad = contratos.Count,
            Activos = contratos.Count(x => x.Estado == EstadoContrato.Activo || x.Estado == EstadoContrato.Renovado),
            Finalizados = contratos.Count(x => x.Estado == EstadoContrato.Finalizado),
            Cancelados = contratos.Count(x => x.Estado == EstadoContrato.Cancelado),
            RenovacionAutomatica = contratos.Count(x => x.RenovacionAutomatica),
            ImporteContratos = contratos.Sum(x => x.Importe),
            ImpactoComercial = impactos.Where(x => x.Tipo == TipoImpactoContrato.Comercial).Sum(x => x.Importe),
            ImpactoFinanciero = impactos.Where(x => x.Tipo == TipoImpactoContrato.Financiero).Sum(x => x.Importe)
        });
    }

    [HttpGet("reportes")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportarReporte([FromQuery] long? terceroId = null, [FromQuery] long? sucursalId = null, [FromQuery] string? estado = null, [FromQuery] bool soloVigentes = false, [FromQuery] FormatoExportacionReporte formato = FormatoExportacionReporte.Csv, CancellationToken ct = default)
    {
        EstadoContrato? estadoEnum = null;
        if (!string.IsNullOrWhiteSpace(estado) && Enum.TryParse<EstadoContrato>(estado, true, out var parsed))
            estadoEnum = parsed;

        var reporte = await contratosService.GetReporteContratosAsync(terceroId, sucursalId, estadoEnum, soloVigentes, ct);
        var archivo = reporteExportacionService.Exportar(reporte, formato, "contratos");
        return File(archivo.Contenido, archivo.ContentType, archivo.NombreArchivo);
    }

    [HttpGet("reportes/vencimientos")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportarReporteVencimientos([FromQuery] long? sucursalId = null, [FromQuery] int dias = 30, [FromQuery] FormatoExportacionReporte formato = FormatoExportacionReporte.Csv, CancellationToken ct = default)
    {
        var reporte = await contratosService.GetReporteVencimientosAsync(sucursalId, dias, ct);
        var archivo = reporteExportacionService.Exportar(reporte, formato, "contratos_vencimientos");
        return File(archivo.Contenido, archivo.ContentType, archivo.NombreArchivo);
    }

    [HttpGet("reportes/historial")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportarReporteHistorial([FromQuery] long? contratoId = null, [FromQuery] long? terceroId = null, [FromQuery] long? sucursalId = null, [FromQuery] string? tipoEvento = null, [FromQuery] DateOnly? desde = null, [FromQuery] DateOnly? hasta = null, [FromQuery] FormatoExportacionReporte formato = FormatoExportacionReporte.Csv, CancellationToken ct = default)
    {
        TipoEventoContrato? tipoEventoEnum = null;
        if (!string.IsNullOrWhiteSpace(tipoEvento) && Enum.TryParse<TipoEventoContrato>(tipoEvento, true, out var parsed))
            tipoEventoEnum = parsed;

        var reporte = await contratosService.GetReporteHistorialAsync(contratoId, terceroId, sucursalId, tipoEventoEnum, desde, hasta, ct);
        var archivo = reporteExportacionService.Exportar(reporte, formato, "contratos_historial");
        return File(archivo.Contenido, archivo.ContentType, archivo.NombreArchivo);
    }

    [HttpGet("reportes/impactos")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportarReporteImpactos([FromQuery] long? contratoId = null, [FromQuery] long? terceroId = null, [FromQuery] long? sucursalId = null, [FromQuery] string? tipoImpacto = null, [FromQuery] DateOnly? desde = null, [FromQuery] DateOnly? hasta = null, [FromQuery] FormatoExportacionReporte formato = FormatoExportacionReporte.Csv, CancellationToken ct = default)
    {
        TipoImpactoContrato? tipoImpactoEnum = null;
        if (!string.IsNullOrWhiteSpace(tipoImpacto) && Enum.TryParse<TipoImpactoContrato>(tipoImpacto, true, out var parsed))
            tipoImpactoEnum = parsed;

        var reporte = await contratosService.GetReporteImpactosAsync(contratoId, terceroId, sucursalId, tipoImpactoEnum, desde, hasta, ct);
        var archivo = reporteExportacionService.Exportar(reporte, formato, "contratos_impactos");
        return File(archivo.Contenido, archivo.ContentType, archivo.NombreArchivo);
    }
}

public record RenovarContratoRequest(DateOnly NuevaFechaFin, decimal? NuevoImporte, string? Observacion, bool GenerarImpactoComercial = true, bool GenerarImpactoFinanciero = true);
public record CancelarContratoRequest(string? Observacion);
public record RegistrarImpactoContratoRequest(TipoImpactoContrato Tipo, DateOnly Fecha, decimal Importe, string Descripcion, bool ImpactarCuentaCorriente = false);
public record FinalizarContratosVencidosRequest(long? SucursalId, DateOnly FechaCorte);
public record RenovarContratosAutomaticamenteRequest(long? SucursalId, DateOnly FechaCorte, decimal? PorcentajeAjuste = null, bool GenerarImpactoComercial = true, bool GenerarImpactoFinanciero = true);
