using ZuluIA_Back.Application.Features.Integraciones.Services;

namespace ZuluIA_Back.Api.HostedServices;

public class BatchSchedulerHostedService(IServiceScopeFactory scopeFactory, ILogger<BatchSchedulerHostedService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var delay = TimeSpan.FromMinutes(1);
            try
            {
                using var scope = scopeFactory.CreateScope();
                var service = scope.ServiceProvider.GetRequiredService<BatchSchedulerService>();
                var settings = scope.ServiceProvider.GetRequiredService<OperacionesBatchSettingsService>();
                var resolved = await settings.ResolveAsync(stoppingToken);
                delay = TimeSpan.FromSeconds(Math.Max(5, resolved.SchedulerPollSeconds));
                await service.ProcesarPendientesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error procesando scheduler batch.");
            }

            await Task.Delay(delay, stoppingToken);
        }
    }
}
