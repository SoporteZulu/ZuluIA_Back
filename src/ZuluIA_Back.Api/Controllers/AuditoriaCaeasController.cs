using MediatR;
using Microsoft.AspNetCore.Mvc;
using ZuluIA_Back.Application.Features.Auditoria.Commands;
using ZuluIA_Back.Application.Features.Auditoria.Queries;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Api.Controllers;

[Route("api/auditoria-caeas")]
public class AuditoriaCaeasController(IMediator mediator) : BaseController(mediator)
{
    [HttpGet("{caeaId:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByCaea(long caeaId, CancellationToken ct = default)
        => Ok(await Mediator.Send(new GetAuditoriaCaeaQuery(caeaId), ct));

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Registrar([FromBody] RegistrarAuditoriaCaeaRequest request, CancellationToken ct = default)
    {
        if (!Enum.TryParse<AccionAuditoria>(request.Accion, true, out var accion))
            return BadRequest(new { error = "Accion inválida." });

        await Mediator.Send(new RegistrarAuditoriaCaeaCommand(
            request.CaeaId,
            request.UsuarioId,
            accion,
            request.DetalleCambio,
            request.IpOrigen), ct);

        return Ok();
    }
}

public record RegistrarAuditoriaCaeaRequest(
    long CaeaId,
    long? UsuarioId,
    string Accion,
    string? DetalleCambio,
    string? IpOrigen);