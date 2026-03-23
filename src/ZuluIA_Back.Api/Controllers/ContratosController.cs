using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Comprobantes.Commands;

namespace ZuluIA_Back.Api.Controllers;

/// <summary>
/// Gestión de contratos de servicio/abono con facturación periódica.
/// Migrado desde VB6: frmContratos / CONTRATOSDETALLES.
/// </summary>
public class ContratosController(IMediator mediator, IApplicationDbContext db)
    : BaseController(mediator)
{
    // GET api/contratos
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] long? terceroId,
        [FromQuery] string? estado,
        [FromQuery] bool incluirAnulados = false,
        CancellationToken ct = default)
    {
        var query = db.Contratos.AsNoTracking();

        if (!incluirAnulados) query = query.Where(x => !x.Anulado);
        if (terceroId.HasValue) query = query.Where(x => x.TerceroId == terceroId.Value);
        if (!string.IsNullOrWhiteSpace(estado)) query = query.Where(x => x.Estado == estado.ToUpperInvariant());

        var contratos = await query
            .OrderByDescending(x => x.FechaDesde)
            .Select(x => new
            {
                x.Id,
                x.TerceroId,
                x.VendedorId,
                x.FechaDesde,
                x.FechaVencimiento,
                x.FechaInicioFacturacion,
                x.PeriodoMeses,
                x.Duracion,
                x.CuotasRestantes,
                x.Estado,
                x.Anulado,
                x.Total,
                x.Observacion,
                x.CreatedAt
            })
            .ToListAsync(ct);

        return Ok(contratos);
    }

    // GET api/contratos/{id}
    [HttpGet("{id:long}", Name = "GetContratoById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
    {
        var contrato = await db.Contratos.AsNoTracking()
            .Include(x => x.Detalles)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (contrato is null)
            return NotFound(new { error = $"Contrato {id} no encontrado." });

        return Ok(new
        {
            contrato.Id,
            contrato.TerceroId,
            contrato.SucursalTerceroId,
            contrato.VendedorId,
            contrato.TipoComprobanteId,
            contrato.PuntoFacturacionId,
            contrato.CondicionVentaId,
            contrato.MonedaId,
            contrato.Cotizacion,
            contrato.FechaDesde,
            contrato.FechaVencimiento,
            contrato.FechaInicioFacturacion,
            contrato.PeriodoMeses,
            contrato.Duracion,
            contrato.CuotasRestantes,
            contrato.Estado,
            contrato.Anulado,
            contrato.Total,
            contrato.Observacion,
            contrato.CreatedAt,
            contrato.UpdatedAt,
            Detalles = contrato.Detalles.Select(d => new
            {
                d.Id,
                d.ItemId,
                d.Descripcion,
                d.Cantidad,
                d.PrecioUnitario,
                d.PorcentajeIva,
                d.Total,
                d.FechaDesde,
                d.FechaHasta,
                d.FechaPrimeraFactura,
                d.FrecuenciaMeses,
                d.Corte,
                d.Dominio,
                d.Estado,
                d.Anulado
            })
        });
    }

    // POST api/contratos
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CrearContratoRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new CreateContratoCommand(
                req.TerceroId,
                req.SucursalTerceroId,
                req.VendedorId,
                req.TipoComprobanteId,
                req.PuntoFacturacionId,
                req.CondicionVentaId,
                req.MonedaId,
                req.Cotizacion,
                req.FechaDesde,
                req.FechaVencimiento,
                req.FechaInicioFacturacion,
                req.PeriodoMeses,
                req.Duracion,
                req.Total,
                req.Observacion,
                req.Detalles.Select(x => new CreateContratoDetalleInput(
                    x.ItemId,
                    x.Descripcion,
                    x.Cantidad,
                    x.PrecioUnitario,
                    x.PorcentajeIva,
                    x.FechaDesde,
                    x.FechaHasta,
                    x.FechaPrimeraFactura,
                    x.FrecuenciaMeses,
                    x.Corte,
                    x.Dominio)).ToList()),
            ct);

        return result.IsSuccess
            ? CreatedAtRoute("GetContratoById", new { id = result.Value }, new { id = result.Value })
            : BadRequest(new { error = result.Error });
    }

    // PUT api/contratos/{id}
    [HttpPut("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(long id, [FromBody] ActualizarContratoRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new UpdateContratoCommand(
                id,
                req.VendedorId,
                req.CondicionVentaId,
                req.FechaVencimiento,
                req.PeriodoMeses,
                req.Duracion,
                req.Total,
                req.Observacion),
            ct);

        if (!result.IsSuccess)
        {
            var error = result.Error ?? "No fue posible actualizar el contrato.";
            return error.Contains("no encontrado", StringComparison.OrdinalIgnoreCase)
                ? NotFound(new { error })
                : BadRequest(new { error });
        }

        return Ok(new { id });
    }

    // POST api/contratos/{id}/anular
    [HttpPost("{id:long}/anular")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Anular(long id, [FromBody] AnularContratoRequest? req, CancellationToken ct)
    {
        var result = await Mediator.Send(new AnularContratoCommand(id, req?.Motivo), ct);
        if (!result.IsSuccess)
        {
            var error = result.Error ?? "No fue posible anular el contrato.";
            return error.Contains("no encontrado", StringComparison.OrdinalIgnoreCase)
                ? NotFound(new { error })
                : BadRequest(new { error });
        }

        return Ok(new { id = result.Value.Id, estado = result.Value.Estado });
    }

    // POST api/contratos/{id}/facturar
    /// <summary>Registra una facturación periódica reduciendo cuotas restantes.</summary>
    [HttpPost("{id:long}/facturar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RegistrarFacturacion(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new RegistrarFacturacionContratoCommand(id), ct);
        if (!result.IsSuccess)
        {
            var error = result.Error ?? "No fue posible registrar la facturación del contrato.";
            return error.Contains("no encontrado", StringComparison.OrdinalIgnoreCase)
                ? NotFound(new { error })
                : BadRequest(new { error });
        }

        return Ok(new
        {
            id = result.Value.Id,
            cuotasRestantes = result.Value.CuotasRestantes,
            estado = result.Value.Estado
        });
    }

    // GET api/contratos/vencidos
    [HttpGet("vencidos")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetVencidos(CancellationToken ct)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var vencidos = await db.Contratos.AsNoTracking()
            .Where(x => !x.Anulado && x.Estado == "VIGENTE" && x.FechaVencimiento < today)
            .Select(x => new { x.Id, x.TerceroId, x.FechaVencimiento, x.Total })
            .ToListAsync(ct);
        return Ok(vencidos);
    }
}

// ── DTOs ──────────────────────────────────────────────────────────────────────

public record CrearContratoRequest(
    long TerceroId,
    long? SucursalTerceroId,
    long? VendedorId,
    long? TipoComprobanteId,
    long? PuntoFacturacionId,
    int? CondicionVentaId,
    int? MonedaId,
    decimal Cotizacion,
    DateOnly FechaDesde,
    DateOnly FechaVencimiento,
    DateOnly FechaInicioFacturacion,
    int PeriodoMeses,
    int Duracion,
    decimal Total,
    string? Observacion,
    IReadOnlyList<ContratoDetalleRequest> Detalles);

public record ContratoDetalleRequest(
    long? ItemId,
    string Descripcion,
    decimal Cantidad,
    decimal PrecioUnitario,
    decimal? PorcentajeIva,
    DateOnly FechaDesde,
    DateOnly FechaHasta,
    DateOnly FechaPrimeraFactura,
    int FrecuenciaMeses,
    int Corte,
    string? Dominio);

public record ActualizarContratoRequest(
    long? VendedorId,
    int? CondicionVentaId,
    DateOnly FechaVencimiento,
    int PeriodoMeses,
    int Duracion,
    decimal Total,
    string? Observacion);

public record AnularContratoRequest(string? Motivo);
