using System.Security.Claims;
using ZuluIA_Back.Application.Common.Interfaces;

namespace ZuluIA_Back.Api.Middleware;

public class CurrentUserMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, ICurrentUserService currentUserService)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier)
                      ?? context.User.FindFirstValue("sub");
            var email = context.User.FindFirstValue(ClaimTypes.Email)
                      ?? context.User.FindFirstValue("email");

            if (currentUserService is CurrentUserService svc)
                svc.SetUser(userId, email);
        }

        await next(context);
    }
}
