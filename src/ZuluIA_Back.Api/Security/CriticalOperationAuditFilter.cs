using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.Filters;
using ZuluIA_Back.Api.Middleware;
using ZuluIA_Back.Application.Common.Interfaces;

namespace ZuluIA_Back.Api.Security;

public class CriticalOperationAuditFilter(
    ICurrentUserService currentUserService,
    ILogger<CriticalOperationAuditFilter> logger) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var operation = context.ActionDescriptor.EndpointMetadata.OfType<AuditCriticalOperationAttribute>().LastOrDefault()?.OperationName
            ?? context.ActionDescriptor.DisplayName
            ?? "CRITICAL_OPERATION";
        var correlationId = context.HttpContext.Items.TryGetValue(CorrelationIdMiddleware.ItemKey, out var value)
            ? value?.ToString()
            : null;
        var stopwatch = Stopwatch.StartNew();

        logger.LogInformation(
            "Operacion critica iniciada {Operation} {Method} {Path} UserId={UserId} CorrelationId={CorrelationId}",
            operation,
            context.HttpContext.Request.Method,
            context.HttpContext.Request.Path,
            currentUserService.UserId,
            correlationId);

        var executed = await next();
        stopwatch.Stop();

        if (executed.Exception is null || executed.ExceptionHandled)
        {
            logger.LogInformation(
                "Operacion critica finalizada {Operation} StatusCode={StatusCode} ElapsedMs={ElapsedMs} UserId={UserId} CorrelationId={CorrelationId}",
                operation,
                context.HttpContext.Response.StatusCode,
                stopwatch.ElapsedMilliseconds,
                currentUserService.UserId,
                correlationId);
            return;
        }

        logger.LogError(
            executed.Exception,
            "Operacion critica fallida {Operation} StatusCode={StatusCode} ElapsedMs={ElapsedMs} UserId={UserId} CorrelationId={CorrelationId}",
            operation,
            context.HttpContext.Response.StatusCode,
            stopwatch.ElapsedMilliseconds,
            currentUserService.UserId,
            correlationId);
    }
}
