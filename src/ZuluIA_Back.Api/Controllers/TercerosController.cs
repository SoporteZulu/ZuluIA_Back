using MediatR;
using Microsoft.AspNetCore.Mvc;
using ZuluIA_Back.Application.Features.Terceros.Commands;
using ZuluIA_Back.Application.Features.Terceros.Queries;

namespace ZuluIA_Back.Api.Controllers;

public class TercerosController(IMediator mediator) : BaseController(mediator)
{
    // ─────────────────────────────────────────────────────────────────────────
    // QUERIES
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Listado paginado de terceros con filtros combinables.
    /// Equivalente a la grilla del ABM de Clientes/Proveedores del VB6.
    /// GET /api/terceros?page=1&pageSize=20&search=garcia&soloClientes=true
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] bool? soloClientes = null,
        [FromQuery] bool? soloProveedores = null,
        [FromQuery] bool? soloEmpleados = null,
        [FromQuery] bool? soloActivos = true,
        [FromQuery] long? condicionIvaId = null,
        [FromQuery] long? categoriaId = null,
        [FromQuery] long? sucursalId = null,
        CancellationToken ct = default)
    {
        var result = await Mediator.Send(
            new GetTercerosPagedQuery(
                page,
                pageSize,
                search,
                soloClientes,
                soloProveedores,
                soloEmpleados,
                soloActivos,
                condicionIvaId,
                categoriaId,
                sucursalId),
            ct);

        return Ok(result);
    }

    /// <summary>
    /// Detalle completo de un tercero por Id.
    /// Equivalente a abrir el formulario de ABM al hacer doble click en una fila.
    /// GET /api/terceros/42
    /// </summary>
    [HttpGet("{id:long}", Name = "GetTerceroById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetTerceroByIdQuery(id), ct);
        return FromResult(result);
    }

    /// <summary>
    /// Busca un tercero por su legajo (identificador de negocio).
    /// Equivalente a la búsqueda rápida por legajo en el VB6.
    /// GET /api/terceros/legajo/CLI001
    /// </summary>
    [HttpGet("legajo/{legajo}", Name = "GetTerceroByLegajo")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByLegajo(string legajo, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetTerceroByLegajoQuery(legajo), ct);
        return FromResult(result);
    }

    /// <summary>
    /// Lista de clientes activos para combos y selectores.
    /// Equivalente al llenarComboClientes() del VB6.
    /// GET /api/terceros/clientes-activos?sucursalId=1
    /// </summary>
    [HttpGet("clientes-activos")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetClientesActivos(
        [FromQuery] long? sucursalId = null,
        CancellationToken ct = default)
    {
        var result = await Mediator.Send(
            new GetClientesActivosQuery(sucursalId), ct);
        return Ok(result);
    }

    /// <summary>
    /// Lista de proveedores activos para combos y selectores.
    /// Equivalente al llenarComboProveedores() del VB6.
    /// GET /api/terceros/proveedores-activos?sucursalId=1
    /// </summary>
    [HttpGet("proveedores-activos")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProveedoresActivos(
        [FromQuery] long? sucursalId = null,
        CancellationToken ct = default)
    {
        var result = await Mediator.Send(
            new GetProveedoresActivosQuery(sucursalId), ct);
        return Ok(result);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // COMMANDS
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Crea un nuevo tercero.
    /// Equivalente al agregarNuevo() → Guardar() del VB6.
    /// POST /api/terceros
    /// Retorna 201 Created con header Location: /api/terceros/{id}
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateTerceroCommand command,
        CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return CreatedFromResult(
            result,
            "GetTerceroById",
            new { id = result.IsSuccess ? result.Value : 0 });
    }

    /// <summary>
    /// Actualiza los datos de un tercero existente.
    /// Equivalente al Guardar() en modo edición del VB6.
    /// PUT /api/terceros/42
    /// </summary>
    [HttpPut("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        long id,
        [FromBody] UpdateTerceroCommand command,
        CancellationToken ct)
    {
        // Guardia: el Id de la URL debe coincidir con el del body.
        // Patrón del proyecto (ver ItemsController).
        if (id != command.Id)
            return BadRequest(new { error = "El Id de la URL no coincide con el del body." });

        var result = await Mediator.Send(command, ct);
        return FromResult(result);
    }

    /// <summary>
    /// Da de baja lógica (soft delete) a un tercero.
    /// Equivalente al eliminar() + validarEliminar() del VB6.
    /// DELETE /api/terceros/42
    /// Retorna 400 con mensaje si tiene comprobantes, cuenta corriente o empleado activo.
    /// </summary>
    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new DeleteTerceroCommand(id), ct);
        return FromResult(result);
    }

    /// <summary>
    /// Reactiva un tercero dado de baja lógica.
    /// No existía en VB6 (se hacía manual en BD),
    /// pero es necesario para el flujo de administración.
    /// PATCH /api/terceros/42/activar
    /// </summary>
    [HttpPatch("{id:long}/activar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Activar(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new ActivarTerceroCommand(id), ct);
        return FromResult(result);
    }
}