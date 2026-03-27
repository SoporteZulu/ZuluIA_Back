using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Api.Security;

public class PermissionAuthorizationFilter(
    ISeguridadRepository seguridadRepository,
    ICurrentUserService currentUserService,
    ILogger<PermissionAuthorizationFilter> logger) : IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var requirements = context.ActionDescriptor.EndpointMetadata
            .OfType<RequirePermissionAttribute>()
            .Select(x => x.Permission)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (requirements.Count == 0)
            return;

        if (currentUserService.UserId is null)
        {
            context.Result = new ForbidResult();
            return;
        }

        foreach (var permission in requirements)
        {
            var definition = await seguridadRepository.GetByIdentificadorAsync(permission, context.HttpContext.RequestAborted);
            if (definition is null)
            {
                logger.LogWarning("Permiso {Permission} no registrado. Se permite acceso por rollout seguro.", permission);
                continue;
            }

            if (!definition.AplicaSeguridadPorUsuario)
                continue;

            var allowed = await seguridadRepository.TienePermisoAsync(currentUserService.UserId.Value, permission, context.HttpContext.RequestAborted);
            if (allowed)
                continue;

            context.Result = new ObjectResult(new { error = $"Permiso requerido: {permission}." })
            {
                StatusCode = StatusCodes.Status403Forbidden
            };
            return;
        }
    }
}
