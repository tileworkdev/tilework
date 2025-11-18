using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

using Coravel;

using Tilework.LoadBalancing.Interfaces;

namespace Tilework.LoadBalancing.Services;

public sealed class MonitoringInitializer : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MonitoringInitializer> _logger;

    public MonitoringInitializer(ILogger<MonitoringInitializer> logger,
                                 IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public async Task StartAsync(CancellationToken ct)
    {
        _logger.LogInformation($"Initiating startup for module: Monitoring");
        await using var scope = _serviceProvider.CreateAsyncScope();

        var dataCollectorService = scope.ServiceProvider.GetRequiredService<DataCollectorService>();
        await dataCollectorService.ApplyConfiguration();
    }

    public async Task StopAsync(CancellationToken ct)
    {
        _logger.LogInformation($"Initiating shutdown for module: Monitoring");
        await using var scope = _serviceProvider.CreateAsyncScope();
        var dataCollectorService = scope.ServiceProvider.GetRequiredService<DataCollectorService>();

        await dataCollectorService.Shutdown();
    }
}
