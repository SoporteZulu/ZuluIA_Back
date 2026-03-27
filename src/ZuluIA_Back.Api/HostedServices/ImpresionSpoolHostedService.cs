using ZuluIA_Back.Application.Features.Integraciones.Services;

namespace ZuluIA_Back.Api.HostedServices;

public class ImpresionSpoolHostedService(IServiceScopeFactory scopeFactory, ILogger<ImpresionSpoolHostedService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var delay = TimeSpan.FromSeconds(15);
            try
            {
                using var scope = scopeFactory.CreateScope();
                var service = scope.ServiceProvider.GetRequiredService<ImpresionSpoolService>();
                var settings = scope.ServiceProvider.GetRequiredService<OperacionesBatchSettingsService>();
                var resolved = await settings.ResolveAsync(stoppingToken);
                delay = TimeSpan.FromSeconds(Math.Max(5, resolved.SpoolPollSeconds));
                await service.ProcesarPendientesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error procesando spool de impresión.");
            }

            await Task.Delay(delay, stoppingToken);
        }
    }
}
