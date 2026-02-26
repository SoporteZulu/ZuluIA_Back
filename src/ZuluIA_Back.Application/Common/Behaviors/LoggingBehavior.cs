using MediatR;
using Microsoft.Extensions.Logging;

namespace ZuluIA_Back.Application.Common.Behaviors;

public class LoggingBehavior<TRequest, TResponse>(
    ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        var requestName = typeof(TRequest).Name;

        logger.LogInformation("Ejecutando request: {RequestName} {@Request}", requestName, request);

        var sw = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            var response = await next();
            sw.Stop();
            logger.LogInformation("Request {RequestName} completado en {Elapsed}ms", requestName, sw.ElapsedMilliseconds);
            return response;
        }
        catch (Exception ex)
        {
            sw.Stop();
            logger.LogError(ex, "Request {RequestName} falló en {Elapsed}ms", requestName, sw.ElapsedMilliseconds);
            throw;
        }
    }
}