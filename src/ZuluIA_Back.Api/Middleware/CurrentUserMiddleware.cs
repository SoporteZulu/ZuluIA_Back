using System.Security.Claims;
using Serilog.Context;
using ZuluIA_Back.Application.Common.Interfaces;

namespace ZuluIA_Back.Api.Middleware;

public class CurrentUserMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, ICurrentUserService currentUserService)
    {
        IDisposable? userIdScope = null;
        IDisposable? emailScope = null;
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier)
                      ?? context.User.FindFirstValue("sub");
            var email = context.User.FindFirstValue(ClaimTypes.Email)
                      ?? context.User.FindFirstValue("email");

            if (currentUserService is CurrentUserService svc)
                svc.SetUser(userId, email);

            context.Items["CurrentUserId"] = userId;
            context.Items["CurrentUserEmail"] = email;
            userIdScope = LogContext.PushProperty("UserId", userId);
            emailScope = LogContext.PushProperty("UserEmail", email);
        }

        try
        {
            await next(context);
        }
        finally
        {
            emailScope?.Dispose();
            userIdScope?.Dispose();
        }
    }
}
