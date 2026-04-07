using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Terceros.Queries;

namespace ZuluIA_Back.Api.Controllers;

/// <summary>
/// Catálogo legacy de tipos de domicilio.
/// </summary>
public class TiposDomicilioController(IMediator mediator, IApplicationDbContext db)
    : BaseController(mediator)
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await Mediator.Send(new GetTiposDomicilioQuery(), ct);
        return Ok(result);
    }

    [HttpGet("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
    {
        var item = await db.TiposDomicilio.AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new { x.Id, Descripcion = x.Descripcion })
            .FirstOrDefaultAsync(ct);

        return OkOrNotFound(item);
    }
}
