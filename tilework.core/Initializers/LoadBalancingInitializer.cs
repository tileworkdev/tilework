using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

using Coravel;

using Tilework.LoadBalancing.Interfaces;
using Tilework.Core.Jobs.LoadBalancing;

namespace Tilework.LoadBalancing.Services;

public sealed class LoadBalancingInitializer : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<LoadBalancingInitializer> _logger;

    public LoadBalancingInitializer(ILogger<LoadBalancingInitializer> logger,
                                    IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public async Task StartAsync(CancellationToken ct)
    {
        _logger.LogInformation($"Initiating startup for module: LoadBalancing");
        await using var scope = _serviceProvider.CreateAsyncScope();

        var loadBalancerService = scope.ServiceProvider.GetRequiredService<ILoadBalancerService>();
        await loadBalancerService.ApplyConfiguration();

        scope.ServiceProvider.UseScheduler(s =>
        {
            s.Schedule<LoadBalancerMonitoringJob>()
             .EveryMinute()
             .PreventOverlapping("LoadBalancerMonitoringJob");
        });
    }

    public async Task StopAsync(CancellationToken ct)
    {
        _logger.LogInformation($"Initiating shutdown for module: LoadBalancing");
        await using var scope = _serviceProvider.CreateAsyncScope();
        var loadBalancerService = scope.ServiceProvider.GetRequiredService<ILoadBalancerService>();

        await loadBalancerService.Shutdown();
    }
}
