using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZuluIA_Back.Application.Common.Interfaces;

namespace ZuluIA_Back.Api.Controllers;

public class AuthController(IMediator mediator, ICurrentUserService currentUser)
    : BaseController(mediator)
{
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult Me()
    {
        if (!currentUser.IsAuthenticated)
            return Unauthorized();

        return Ok(new
        {
            userId = currentUser.UserId,
            email = currentUser.Email
        });
    }

    [HttpGet("ping")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Ping() =>
        Ok(new { message = "ZuluIA_Back API funcionando", timestamp = DateTime.UtcNow });
}