using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Usuarios.Commands;
using ZuluIA_Back.Domain.Entities.Usuarios;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Api.Controllers;

public class SeguridadController(
    IMediator mediator,
    ISeguridadRepository repo,
    IApplicationDbContext db)
    : BaseController(mediator)
{
    /// <summary>
    /// Retorna todos los permisos/funciones del sistema registrados.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var permisos = await db.Seguridad
            .AsNoTracking()
            .OrderBy(x => x.Identificador)
            .Select(x => new
            {
                x.Id,
                x.Identificador,
                x.Descripcion,
                x.AplicaSeguridadPorUsuario
            })
            .ToListAsync(ct);

        return Ok(permisos);
    }

    /// <summary>
    /// Crea un nuevo permiso del sistema.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateSeguridadRequest request,
        CancellationToken ct)
    {
        var result = await Mediator.Send(
            new CreateSeguridadCommand(
                request.Identificador,
                request.Descripcion,
                request.AplicaSeguridadPorUsuario),
            ct);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(new { id = result.Value });
    }

    /// <summary>
    /// Verifica si un usuario tiene un permiso específico.
    /// </summary>
    [HttpGet("verificar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Verificar(
        [FromQuery] long usuarioId,
        [FromQuery] string identificador,
        CancellationToken ct)
    {
        var tiene = await repo.TienePermisoAsync(usuarioId, identificador, ct);
        return Ok(new
        {
            usuarioId,
            identificador,
            tienePermiso = tiene
        });
    }

    /// <summary>
    /// Retorna el mapa completo de permisos de un usuario (clave → valor).
    /// </summary>
    [HttpGet("usuario/{usuarioId:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPermisosUsuario(
        long usuarioId,
        CancellationToken ct)
    {
        var permisos = await repo.GetPermisosUsuarioAsync(usuarioId, ct);
        return Ok(permisos);
    }
}

public record CreateSeguridadRequest(
    string Identificador,
    string? Descripcion,
    bool AplicaSeguridadPorUsuario);