using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
[Produces("application/json")]
public abstract class BaseController(IMediator mediator) : ControllerBase
{
    protected readonly IMediator Mediator = mediator;

    protected IActionResult OkOrNotFound<T>(T? value) =>
        value is null ? NotFound() : Ok(value);

    protected IActionResult FromResult(Result result) =>
        result.IsSuccess ? Ok() : BadRequest(new { error = result.Error });

    protected IActionResult FromResult<T>(Result<T> result) =>
        result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(new { error = result.Error });

    protected IActionResult CreatedFromResult<T>(Result<T> result, string routeName, object routeValues) =>
        result.IsSuccess
            ? CreatedAtRoute(routeName, routeValues, new { id = result.Value })
            : BadRequest(new { error = result.Error });
}
