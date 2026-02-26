using System.Net;
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
            logger.LogError(ex, "Error no manejado: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

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

        var response = new
        {
            status = (int)statusCode,
            message,
            detail = exception.Message
        };

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(response,
                new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                }));
    }
}