using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Cajas.Commands;
using ZuluIA_Back.Application.Features.Cajas.Queries;
using ZuluIA_Back.Application.Features.Cajas.DTOs;
using ZuluIA_Back.Application.Features.Finanzas.Commands;

namespace ZuluIA_Back.Api.Controllers;

public class CajasController(IMediator mediator, IApplicationDbContext db)
    : BaseController(mediator)
{
    /// <summary>
    /// Retorna todas las cajas/cuentas activas de una sucursal.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBySucursal(
        [FromQuery] long sucursalId,
        CancellationToken ct)
    {
        var result = await Mediator.Send(new GetCajasBySucursalQuery(sucursalId), ct);
        return Ok(result);
    }

    /// <summary>
    /// Retorna el detalle completo de una caja/cuenta por ID.
    /// </summary>
    [HttpGet("{id:long}", Name = "GetCajaById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetCajaByIdQuery(id), ct);
        return OkOrNotFound(result);
    }

    /// <summary>
    /// Retorna los tipos de caja/cuenta disponibles.
    /// </summary>
    [HttpGet("tipos")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTipos(CancellationToken ct)
    {
        var tipos = await db.TiposCajaCuenta
            .AsNoTracking()
            .OrderBy(x => x.Descripcion)
            .Select(x => new { x.Id, x.Descripcion, x.EsCaja })
            .ToListAsync(ct);

        return Ok(tipos);
    }

    /// <summary>
    /// Retorna las formas de pago habilitadas para una caja.
    /// </summary>
    [HttpGet("{id:long}/formas-pago")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFormasPago(long id, CancellationToken ct)
    {
        var formas = await db.FormasPagoCaja
            .AsNoTracking()
            .Where(x => x.CajaId == id && x.Habilitado)
            .Select(x => new
            {
                x.Id,
                x.CajaId,
                x.FormaPagoId,
                x.MonedaId,
                x.Habilitado
            })
            .ToListAsync(ct);

        return Ok(formas);
    }

    /// <summary>
    /// Crea una nueva caja o cuenta bancaria.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateCajaCommand command,
        CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return CreatedFromResult(result, "GetCajaById",
            new { id = result.IsSuccess ? result.Value : 0 });
    }

    /// <summary>
    /// Actualiza una caja/cuenta existente.
    /// </summary>
    [HttpPut("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        long id,
        [FromBody] UpdateCajaCommand command,
        CancellationToken ct)
    {
        if (id != command.Id)
            return BadRequest(new { error = "El ID de la URL no coincide con el del body." });

        var result = await Mediator.Send(command, ct);
        return FromResult(result);
    }

    /// <summary>
    /// Desactiva (soft delete) una caja/cuenta.
    /// </summary>
    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new DeleteCajaCommand(id), ct);
        return FromResult(result);
    }

    /// <summary>
    /// Reactiva una caja/cuenta desactivada.
    /// </summary>
    [HttpPatch("{id:long}/activar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Activar(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new ActivateCajaCommand(id), ct);
        return FromResult(result);
    }

    /// <summary>
    /// Ejecuta el cierre de arqueo de una caja, incrementando el número de cierre.
    /// </summary>
    [HttpPost("{id:long}/cerrar-arqueo")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CerrarArqueo(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new CerrarArqueoCajaCommand(id), ct);
        if (result.IsFailure)
            return result.Error?.Contains("No se encontro", StringComparison.OrdinalIgnoreCase) == true
                ? NotFound(new { error = result.Error })
                : BadRequest(new { error = result.Error });

        var nroCierre = result.Value;

        return Ok(new
        {
            cajaId = id,
            nroCierre,
            mensaje = $"Arqueo cerrado. Número de cierre: {nroCierre}."
        });
    }

    /// <summary>
    /// Registra una apertura de caja (habilita la caja para operar en la fecha).
    /// Equivale a frmAperturaCajasCuentasBancarias del VB6.
    /// </summary>
    [HttpPost("{id:long}/abrir")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AbrirCaja(
        long id,
        [FromBody] AbrirCajaRequest request,
        CancellationToken ct)
    {
        var result = await Mediator.Send(
            new AbrirCajaCommand(id, request.FechaApertura, request.SaldoInicial),
            ct);

        if (result.IsFailure)
            return result.Error?.Contains("No se encontro", StringComparison.OrdinalIgnoreCase) == true
                ? NotFound(new { error = result.Error })
                : BadRequest(new { error = result.Error });

        return Ok(new
        {
            cajaId = id,
            fechaApertura = request.FechaApertura,
            saldoInicial = request.SaldoInicial,
            mensaje = "Caja abierta correctamente."
        });
    }

    /// <summary>
    /// Registra una transferencia entre dos cajas/cuentas bancarias.
    /// Equivale a frmTransferenciasCajasCuentasBancarias del VB6.
    /// </summary>
    [HttpPost("transferencias")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Transferencia(
        [FromBody] RegistrarTransferenciaCommand command,
        CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return result.IsSuccess
            ? Ok(new { id = result.Value })
            : BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Retorna el historial de transferencias de una caja.
    /// </summary>
    [HttpGet("{id:long}/transferencias")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTransferencias(
        long id,
        [FromQuery] DateOnly? desde,
        [FromQuery] DateOnly? hasta,
        CancellationToken ct)
    {
        var query = db.TransferenciasCaja
            .AsNoTracking()
            .Where(x => (x.CajaOrigenId == id || x.CajaDestinoId == id) && !x.Anulada);

        if (desde.HasValue)
            query = query.Where(x => x.Fecha >= desde.Value);
        if (hasta.HasValue)
            query = query.Where(x => x.Fecha <= hasta.Value);

        var lista = await query
            .OrderByDescending(x => x.Fecha)
            .Select(x => new
            {
                x.Id,
                x.Fecha,
                x.CajaOrigenId,
                x.CajaDestinoId,
                x.Importe,
                x.MonedaId,
                x.Cotizacion,
                x.Concepto,
                Tipo = x.CajaOrigenId == id ? "EGRESO" : "INGRESO"
            })
            .ToListAsync(ct);

        return Ok(lista);
    }

    // ── Bancos ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Retorna la lista de bancos configurados.
    /// </summary>
    [HttpGet("bancos")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBancos(CancellationToken ct)
    {
        var lista = await db.Bancos
            .OrderBy(b => b.Descripcion)
            .Select(b => new { b.Id, b.Descripcion })
            .ToListAsync(ct);
        return Ok(lista);
    }

    /// <summary>
    /// Retorna un banco por ID.
    /// </summary>
    [HttpGet("bancos/{id:long}", Name = "GetBancoById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBancoById(long id, CancellationToken ct)
    {
        var banco = await db.Bancos.FindAsync([id], ct);
        return banco is null ? NotFound(new { error = $"Banco {id} no encontrado." }) : Ok(banco);
    }

    /// <summary>
    /// Crea un nuevo banco.
    /// </summary>
    [HttpPost("bancos")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateBanco([FromBody] CreateBancoRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(new CreateBancoCommand(req.Descripcion), ct);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        var banco = await db.Bancos
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == result.Value, ct);

        return CreatedAtRoute("GetBancoById", new { id = result.Value }, new { Id = result.Value, banco?.Descripcion });
    }

    /// <summary>
    /// Actualiza la descripción de un banco.
    /// </summary>
    [HttpPut("bancos/{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateBanco(long id, [FromBody] CreateBancoRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(new UpdateBancoCommand(id, req.Descripcion), ct);
        if (!result.IsSuccess)
            return result.Error?.Contains("no encontrado", StringComparison.OrdinalIgnoreCase) == true
                ? NotFound(new { error = $"Banco {id} no encontrado." })
                : BadRequest(new { error = result.Error });

        return Ok(new { result.Value.Id, result.Value.Descripcion });
    }

    /// <summary>
    /// Elimina un banco (solo si no está en uso).
    /// </summary>
    [HttpDelete("bancos/{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteBanco(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new DeleteBancoCommand(id), ct);
        if (!result.IsSuccess)
            return result.Error?.Contains("no encontrado", StringComparison.OrdinalIgnoreCase) == true
                ? NotFound(new { error = $"Banco {id} no encontrado." })
                : BadRequest(new { error = result.Error });

        return Ok();
    }

    // ── Cierres de caja ───────────────────────────────────────────────────────

    /// <summary>
    /// Retorna los cierres de caja registrados, opcionalmente filtrados por usuario.
    /// </summary>
    [HttpGet("cierres")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCierres(
        [FromQuery] long? usuarioId,
        CancellationToken ct)
    {
        var q = db.CierresCaja.AsQueryable();
        if (usuarioId.HasValue)
            q = q.Where(c => c.UsuarioId == usuarioId.Value);

        var lista = await q
            .OrderByDescending(c => c.FechaCierre)
            .Select(c => new
            {
                c.Id,
                c.NroCierre,
                c.FechaApertura,
                c.FechaCierre,
                c.UsuarioId,
                c.FechaAlta,
                c.FechaControlTesoreria
            })
            .ToListAsync(ct);
        return Ok(lista);
    }

    /// <summary>
    /// Retorna el detalle de un cierre de caja por ID.
    /// </summary>
    [HttpGet("cierres/{id:long}", Name = "GetCierreCajaById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCierreCajaById(long id, CancellationToken ct)
    {
        var cierre = await db.CierresCaja.FindAsync([id], ct);
        return cierre is null ? NotFound(new { error = $"Cierre de caja {id} no encontrado." }) : Ok(cierre);
    }

    /// <summary>
    /// Registra un nuevo cierre de caja.
    /// </summary>
    [HttpPost("cierres")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateCierreCaja([FromBody] RegistrarCierreRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(new CreateCierreCajaCommand(req.FechaApertura, req.FechaCierre, req.UsuarioId, req.NroCierre), ct);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return CreatedAtRoute("GetCierreCajaById", new { id = result.Value }, new { Id = result.Value });
    }

    /// <summary>
    /// Registra el control de tesorería sobre un cierre de caja.
    /// </summary>
    [HttpPatch("cierres/{id:long}/control-tesoreria")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RegistrarControlTesoreria(
        long id,
        [FromBody] RegistrarControlTesoreriaRequest req,
        CancellationToken ct)
    {
        var result = await Mediator.Send(new RegistrarControlTesoreriaCierreCajaCommand(id, req.Fecha), ct);
        if (!result.IsSuccess)
            return NotFound(new { error = $"Cierre de caja {id} no encontrado." });

        return Ok();
    }

    // ── Detalle de cierre de caja ─────────────────────────────────────────────

    /// <summary>
    /// Retorna las líneas de detalle de un cierre de caja (qué cajas/cuentas participaron y diferencias).
    /// Equivale a clsCierresCajasDetalle / CierresCajasDetalle del VB6.
    /// </summary>
    [HttpGet("cierres/{id:long}/detalle")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCierreDetalle(long id, CancellationToken ct)
    {
        var lineas = await db.CierresCajaDetalle
            .AsNoTracking()
            .Where(d => d.CierreId == id)
            .Select(d => new
            {
                d.Id,
                d.CierreId,
                d.CajaCuentaBancariaId,
                d.Diferencia
            })
            .ToListAsync(ct);
        return Ok(lineas);
    }

    /// <summary>
    /// Agrega una línea de detalle a un cierre de caja existente.
    /// </summary>
    [HttpPost("cierres/{id:long}/detalle")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddCierreDetalle(
        long id,
        [FromBody] AddCierreDetalleRequest req,
        CancellationToken ct)
    {
        var result = await Mediator.Send(new AddCierreCajaDetalleCommand(id, req.CajaCuentaBancariaId, req.Diferencia), ct);
        if (!result.IsSuccess)
            return NotFound(new { error = $"Cierre de caja {id} no encontrado." });

        return CreatedAtAction(nameof(GetCierreDetalle), new { id }, new { Id = result.Value });
    }

    /// <summary>
    /// Elimina una línea de detalle de cierre.
    /// </summary>
    [HttpDelete("cierres/{id:long}/detalle/{detalleId:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveCierreDetalle(long id, long detalleId, CancellationToken ct)
    {
        var result = await Mediator.Send(new RemoveCierreCajaDetalleCommand(id, detalleId), ct);
        if (!result.IsSuccess)
            return NotFound(new { error = "Linea de detalle no encontrada." });

        return Ok();
    }
}

// ── Request bodies ───────────────────────────────────────────────────────────
public record AbrirCajaRequest(DateOnly FechaApertura, decimal SaldoInicial);
public record CreateBancoRequest(string Descripcion);
public record RegistrarCierreRequest(DateTimeOffset FechaApertura, DateTimeOffset FechaCierre, long UsuarioId, int NroCierre);
public record RegistrarControlTesoreriaRequest(DateTimeOffset Fecha);
public record AddCierreDetalleRequest(long CajaCuentaBancariaId, decimal Diferencia);