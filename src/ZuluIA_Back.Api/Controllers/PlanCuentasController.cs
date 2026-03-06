using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Contabilidad.Commands;
using ZuluIA_Back.Application.Features.Contabilidad.Queries;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Api.Controllers;

public class PlanCuentasController(
    IMediator mediator,
    IPlanCuentasRepository repo,
    IApplicationDbContext db)
    : BaseController(mediator)
{
    /// <summary>
    /// Retorna el plan de cuentas de un ejercicio en formato árbol.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetArbol(
        [FromQuery] long ejercicioId,
        [FromQuery] bool soloImputables = false,
        CancellationToken ct = default)
    {
        var result = await Mediator.Send(
            new GetPlanCuentasQuery(ejercicioId, soloImputables), ct);
        return Ok(result);
    }

    /// <summary>
    /// Retorna el plan de cuentas plano (sin árbol) para uso en selectores.
    /// </summary>
    [HttpGet("plano")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPlano(
        [FromQuery] long ejercicioId,
        [FromQuery] bool soloImputables = true,
        CancellationToken ct = default)
    {
        var cuentas = soloImputables
            ? await repo.GetImputablesAsync(ejercicioId, ct)
            : await repo.GetByEjercicioAsync(ejercicioId, ct);

        var resultado = cuentas.Select(c => new
        {
            c.Id,
            c.EjercicioId,
            c.IntegradoraId,
            c.CodigoCuenta,
            c.Denominacion,
            c.Nivel,
            c.OrdenNivel,
            c.Imputable,
            c.Tipo,
            c.SaldoNormal
        });

        return Ok(resultado);
    }

    /// <summary>
    /// Busca cuentas por código o denominación.
    /// </summary>
    [HttpGet("buscar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Buscar(
        [FromQuery] long ejercicioId,
        [FromQuery] string termino,
        [FromQuery] bool soloImputables = true,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(termino) || termino.Length < 2)
            return BadRequest(new { error = "El término de búsqueda debe tener al menos 2 caracteres." });

        var query = db.PlanCuentas.AsNoTracking()
            .Where(x => x.EjercicioId == ejercicioId);

        if (soloImputables)
            query = query.Where(x => x.Imputable);

        var terminoLower = termino.Trim().ToLowerInvariant();
        query = query.Where(x =>
            x.CodigoCuenta.ToLower().Contains(terminoLower) ||
            x.Denominacion.ToLower().Contains(terminoLower));

        var resultado = await query
            .OrderBy(x => x.CodigoCuenta)
            .Take(30)
            .Select(x => new
            {
                x.Id,
                x.CodigoCuenta,
                x.Denominacion,
                x.Nivel,
                x.Imputable,
                x.Tipo,
                x.SaldoNormal
            })
            .ToListAsync(ct);

        return Ok(resultado);
    }

    /// <summary>
    /// Crea una nueva cuenta en el plan de cuentas.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreatePlanCuentaCommand command,
        CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return result.IsSuccess
            ? Ok(new { id = result.Value })
            : BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Actualiza los datos de una cuenta contable.
    /// </summary>
    [HttpPut("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        long id,
        [FromBody] UpdatePlanCuentaRequest request,
        CancellationToken ct)
    {
        var cuenta = await repo.GetByIdAsync(id, ct);
        if (cuenta is null)
            return NotFound(new { error = $"No se encontró la cuenta con ID {id}." });

        cuenta.Actualizar(
            request.Denominacion,
            request.Imputable,
            request.Tipo,
            request.SaldoNormal);

        repo.Update(cuenta);
        await db.SaveChangesAsync(ct);

        return Ok(new { mensaje = "Cuenta actualizada correctamente." });
    }
}

public record UpdatePlanCuentaRequest(
    string Denominacion,
    bool Imputable,
    string? Tipo,
    char? SaldoNormal);