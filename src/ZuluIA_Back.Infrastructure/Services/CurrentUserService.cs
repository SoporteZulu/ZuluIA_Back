using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using ZuluIA_Back.Application.Common.Interfaces;

namespace ZuluIA_Back.Infrastructure.Services;

public class HttpCurrentUserService(IHttpContextAccessor httpContextAccessor)
    : ICurrentUserService
{
    private readonly ClaimsPrincipal? _user =
        httpContextAccessor.HttpContext?.User;

    public long? UserId
    {
        get
        {
            var sub = _user?.FindFirstValue(ClaimTypes.NameIdentifier)
                   ?? _user?.FindFirstValue("sub");

            return long.TryParse(sub, out var id) ? id : null;
        }
    }

    public string? Email =>
        _user?.FindFirstValue(ClaimTypes.Email)
     ?? _user?.FindFirstValue("email");

    public bool IsAuthenticated =>
        _user?.Identity?.IsAuthenticated ?? false;
}