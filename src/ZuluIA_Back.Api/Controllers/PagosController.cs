using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Finanzas.Commands;
using ZuluIA_Back.Application.Features.Finanzas.DTOs;
using ZuluIA_Back.Application.Features.Finanzas.Queries;
using ZuluIA_Back.Application.Features.Pagos.Commands;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Api.Controllers;

public class PagosController(
    IMediator mediator,
    IPagoRepository repo,
    IApplicationDbContext db)
    : BaseController(mediator)
{
    /// <summary>
    /// Retorna pagos paginados con filtros opcionales.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] long? sucursalId = null,
        [FromQuery] long? terceroId = null,
        [FromQuery] DateOnly? desde = null,
        [FromQuery] DateOnly? hasta = null,
        CancellationToken ct = default)
    {
        var result = await repo.GetPagedAsync(
            page, pageSize,
            sucursalId, terceroId,
            desde, hasta, ct);

        var terceroIds = result.Items.Select(x => x.TerceroId).Distinct().ToList();
        var monedaIds = result.Items.Select(x => x.MonedaId).Distinct().ToList();

        var terceros = await db.Terceros.AsNoTracking()
            .Where(x => terceroIds.Contains(x.Id))
            .Select(x => new { x.Id, x.RazonSocial })
            .ToDictionaryAsync(x => x.Id, ct);

        var monedas = await db.Monedas.AsNoTracking()
            .Where(x => monedaIds.Contains(x.Id))
            .Select(x => new { x.Id, x.Simbolo })
            .ToDictionaryAsync(x => x.Id, ct);

        var dtos = result.Items.Select(p => new
        {
            p.Id,
            p.SucursalId,
            p.TerceroId,
            TerceroRazonSocial = terceros.GetValueOrDefault(p.TerceroId)?.RazonSocial ?? "—",
            p.Fecha,
            MonedaSimbolo = monedas.GetValueOrDefault(p.MonedaId)?.Simbolo ?? "$",
            p.Total,
            Estado = p.Estado.ToString().ToUpperInvariant(),
            p.CreatedAt
        });

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
    /// Retorna el detalle completo de un pago con sus medios y retenciones.
    /// </summary>
    [HttpGet("{id:long}", Name = "GetPagoById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
    {
        var pago = await repo.GetByIdConMediosAsync(id, ct);
        if (pago is null)
            return NotFound(new { error = $"No se encontró el pago con ID {id}." });

        var tercero = await db.Terceros.AsNoTracking()
            .Where(x => x.Id == pago.TerceroId)
            .Select(x => new { x.RazonSocial })
            .FirstOrDefaultAsync(ct);

        var moneda = await db.Monedas.AsNoTracking()
            .Where(x => x.Id == pago.MonedaId)
            .Select(x => new { x.Simbolo })
            .FirstOrDefaultAsync(ct);

        var cajaIds = pago.Medios.Select(m => m.CajaId).Distinct().ToList();
        var formaPagoIds = pago.Medios.Select(m => m.FormaPagoId).Distinct().ToList();
        var monedaIds = pago.Medios.Select(m => m.MonedaId).Distinct().ToList();

        var cajas = await db.CajasCuentasBancarias.AsNoTracking()
            .Where(x => cajaIds.Contains(x.Id))
            .Select(x => new { x.Id, x.Descripcion })
            .ToDictionaryAsync(x => x.Id, ct);

        var formasPago = await db.FormasPago.AsNoTracking()
            .Where(x => formaPagoIds.Contains(x.Id))
            .Select(x => new { x.Id, x.Descripcion })
            .ToDictionaryAsync(x => x.Id, ct);

        var monedas = await db.Monedas.AsNoTracking()
            .Where(x => monedaIds.Contains(x.Id))
            .Select(x => new { x.Id, x.Simbolo })
            .ToDictionaryAsync(x => x.Id, ct);

        var retenciones = await db.Retenciones.AsNoTracking()
            .Where(x => x.PagoId == pago.Id)
            .Select(x => new RetencionDto
            {
                Id             = x.Id,
                Tipo           = x.Tipo,
                Importe        = x.Importe,
                NroCertificado = x.NroCertificado,
                Fecha          = x.Fecha
            })
            .ToListAsync(ct);

        var dto = new PagoDto
        {
            Id                 = pago.Id,
            SucursalId         = pago.SucursalId,
            TerceroId          = pago.TerceroId,
            TerceroRazonSocial = tercero?.RazonSocial ?? "—",
            Fecha              = pago.Fecha,
            MonedaId           = pago.MonedaId,
            MonedaSimbolo      = moneda?.Simbolo ?? "$",
            Cotizacion         = pago.Cotizacion,
            Total              = pago.Total,
            Observacion        = pago.Observacion,
            Estado             = pago.Estado.ToString().ToUpperInvariant(),
            CreatedAt          = pago.CreatedAt,
            Medios = pago.Medios.Select(m => new PagoMedioDto
            {
                Id                   = m.Id,
                CajaId               = m.CajaId,
                CajaDescripcion      = cajas.GetValueOrDefault(m.CajaId)?.Descripcion ?? "—",
                FormaPagoId          = m.FormaPagoId,
                FormaPagoDescripcion = formasPago.GetValueOrDefault(m.FormaPagoId)?.Descripcion ?? "—",
                ChequeId             = m.ChequeId,
                Importe              = m.Importe,
                MonedaId             = m.MonedaId,
                MonedaSimbolo        = monedas.GetValueOrDefault(m.MonedaId)?.Simbolo ?? "$",
                Cotizacion           = m.Cotizacion
            }).ToList().AsReadOnly(),
            Retenciones = retenciones.AsReadOnly()
        };

        return Ok(dto);
    }

    /// <summary>
    /// Registra un pago con múltiples medios, retenciones
    /// e imputa opcionalmente comprobantes pendientes.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Registrar(
        [FromBody] RegistrarPagoCommand command,
        CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return CreatedFromResult(result, "GetPagoById",
            new { id = result.IsSuccess ? result.Value : 0 });
    }

    /// <summary>
    /// Registra un pago básico con medios, sin retenciones ni imputaciones.
    /// </summary>
    [HttpPost("basico")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegistrarBasico(
        [FromBody] CreatePagoCommand command,
        CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return CreatedFromResult(result, "GetPagoById",
            new { id = result.IsSuccess ? result.Value : 0 });
    }

    /// <summary>
    /// Anula un pago registrado.
    /// </summary>
    [HttpPost("{id:long}/anular")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Anular(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new AnularPagoCommand(id), ct);
        return FromResult(result);
    }
}
