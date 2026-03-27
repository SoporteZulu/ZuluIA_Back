using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Franquicias.Commands;

namespace ZuluIA_Back.Api.Controllers;

[Route("api/planes-trabajo/{planTrabajoId:long}/variables-usuarios")]
public class FranquiciasVariablesUsuariosController(IMediator mediator, IApplicationDbContext db) : BaseController(mediator)
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        long planTrabajoId,
        [FromQuery] long? usuarioId = null,
        [FromQuery] long? variableId = null,
        CancellationToken ct = default)
    {
        var query =
            from fv in db.FranquiciasVariablesXUsuarios.AsNoTracking()
            join u in db.Usuarios.AsNoTracking() on fv.UsuarioId equals u.Id
            join v in db.Variables.AsNoTracking() on fv.VariableId equals v.Id
            where fv.PlanTrabajoId == planTrabajoId
            select new
            {
                fv.Id,
                fv.PlanTrabajoId,
                fv.UsuarioId,
                Usuario = u.NombreCompleto ?? u.UserName,
                fv.VariableId,
                Variable = v.Descripcion,
                fv.Valor
            };

        if (usuarioId.HasValue)
            query = query.Where(x => x.UsuarioId == usuarioId.Value);
        if (variableId.HasValue)
            query = query.Where(x => x.VariableId == variableId.Value);

        var items = await query
            .OrderBy(x => x.Usuario)
            .ThenBy(x => x.Variable)
            .ToListAsync(ct);

        return Ok(items);
    }

    [HttpGet("{id:long}", Name = "GetFranquiciaVariableUsuarioById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(long planTrabajoId, long id, CancellationToken ct)
    {
        var item = await db.FranquiciasVariablesXUsuarios.AsNoTracking()
            .Where(x => x.PlanTrabajoId == planTrabajoId && x.Id == id)
            .Select(x => new
            {
                x.Id,
                x.PlanTrabajoId,
                x.UsuarioId,
                x.VariableId,
                x.Valor
            })
            .FirstOrDefaultAsync(ct);

        return item is null
            ? NotFound(new { error = $"Asignacion de variable por usuario {id} no encontrada." })
            : Ok(item);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(long planTrabajoId, [FromBody] FranquiciaVariableUsuarioRequest request, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new CreateFranquiciaVariableXUsuarioCommand(planTrabajoId, request.UsuarioId, request.VariableId, request.Valor),
            ct);

        if (result.IsFailure)
            return result.Error?.Contains("Ya existe", StringComparison.OrdinalIgnoreCase) == true
                ? Conflict(new { error = result.Error })
                : BadRequest(new { error = result.Error });

        return CreatedAtRoute("GetFranquiciaVariableUsuarioById", new { planTrabajoId, id = result.Value }, new { Id = result.Value });
    }

    [HttpPut("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(long planTrabajoId, long id, [FromBody] UpdateFranquiciaVariableUsuarioRequest request, CancellationToken ct)
    {
        var result = await Mediator.Send(new UpdateFranquiciaVariableXUsuarioCommand(id, request.Valor), ct);
        if (result.IsFailure)
            return result.Error?.Contains("no encontrada", StringComparison.OrdinalIgnoreCase) == true
                ? NotFound(new { error = result.Error })
                : BadRequest(new { error = result.Error });

        return Ok(new { Id = id, PlanTrabajoId = planTrabajoId });
    }

    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(long planTrabajoId, long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new DeleteFranquiciaVariableXUsuarioCommand(id), ct);
        if (result.IsFailure)
            return result.Error?.Contains("no encontrada", StringComparison.OrdinalIgnoreCase) == true
                ? NotFound(new { error = result.Error })
                : BadRequest(new { error = result.Error });

        return Ok(new { Id = id, PlanTrabajoId = planTrabajoId });
    }
}

public record FranquiciaVariableUsuarioRequest(long UsuarioId, long VariableId, string Valor);
public record UpdateFranquiciaVariableUsuarioRequest(string Valor);