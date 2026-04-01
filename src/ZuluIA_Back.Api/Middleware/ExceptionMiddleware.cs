using System.Net;
using System.Reflection;
using System.Text.Json;

namespace ZuluIA_Back.Api.Middleware;

public class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            var effectiveException = Unwrap(ex);
            var correlationId = context.Items.TryGetValue(CorrelationIdMiddleware.ItemKey, out var corr) ? corr?.ToString() : null;
            var userId = context.Items.TryGetValue("CurrentUserId", out var user) ? user?.ToString() : null;
            logger.LogError(effectiveException, "Error no manejado: {Message} CorrelationId={CorrelationId} UserId={UserId}", effectiveException.Message, correlationId, userId);
            await HandleExceptionAsync(context, effectiveException);
        }
    }

    private static Exception Unwrap(Exception exception)
        => exception is TargetInvocationException { InnerException: not null } targetInvocationException
            ? targetInvocationException.InnerException!
            : exception;

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, message) = exception switch
        {
            ArgumentNullException => (HttpStatusCode.BadRequest, "Argumento nulo no permitido."),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "No autorizado."),
            KeyNotFoundException => (HttpStatusCode.NotFound, "Recurso no encontrado."),
            InvalidOperationException => (HttpStatusCode.BadRequest, exception.Message),
            _ => (HttpStatusCode.InternalServerError, "Error interno del servidor.")
        };

        context.Response.StatusCode = (int)statusCode;
        var correlationId = context.Items.TryGetValue(CorrelationIdMiddleware.ItemKey, out var corr) ? corr?.ToString() : null;

        var response = new
        {
            status = (int)statusCode,
            message,
            detail = exception.Message,
            correlationId
        };

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(response,
                new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                }));
    }
}