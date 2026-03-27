using Microsoft.AspNetCore.Mvc;

namespace ZuluIA_Back.Api.Security;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public sealed class RequirePermissionAttribute(string permission) : TypeFilterAttribute(typeof(PermissionAuthorizationFilter))
{
    public string Permission { get; } = permission.Trim().ToUpperInvariant();
}
