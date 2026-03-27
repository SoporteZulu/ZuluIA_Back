using MediatR;
using Microsoft.AspNetCore.Mvc;
using ZuluIA_Back.Application.Features.Caea.Commands;
using ZuluIA_Back.Application.Features.Caea.Queries;

namespace ZuluIA_Back.Api.Controllers;

public class CaeaController(IMediator mediator) : BaseController(mediator)
{
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        [FromQuery] long? puntoFacturacionId = null, [FromQuery] string? estado = null,
        CancellationToken ct = default)
        => Ok(await Mediator.Send(new GetCaeasPagedQuery(page, pageSize, puntoFacturacionId, estado), ct));

    [HttpGet("{id:long}", Name = "GetCaeaById")]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
        => OkOrNotFound(await Mediator.Send(new GetCaeaDetalleQuery(id), ct));

    [HttpPost]
    public async Task<IActionResult> Solicitar([FromBody] SolicitarCaeaCommand command, CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return CreatedFromResult(result, "GetCaeaById", new { id = result.IsSuccess ? result.Value : 0 });
    }

    [HttpPost("solicitar-afip")]
    public async Task<IActionResult> SolicitarAfip([FromBody] SolicitarCaeaAfipRequestDto request, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new SolicitarCaeaAfipCommand(
                request.PuntoFacturacionId,
                request.Periodo,
                request.Orden,
                request.TipoComprobante,
                request.CantidadAsignada),
            ct);

        return CreatedFromResult(result, "GetCaeaById", new { id = result.IsSuccess ? result.Value : 0 });
    }

    [HttpPost("{id:long}/informar")]
    public async Task<IActionResult> Informar(long id, CancellationToken ct)
        => FromResult(await Mediator.Send(new InformarCaeaCommand(id), ct));

    [HttpPost("{id:long}/anular")]
    public async Task<IActionResult> Anular(long id, CancellationToken ct)
        => FromResult(await Mediator.Send(new AnularCaeaCommand(id), ct));
}

public sealed record SolicitarCaeaAfipRequestDto(
    long PuntoFacturacionId,
    int Periodo,
    short Orden,
    string TipoComprobante,
    int CantidadAsignada);
