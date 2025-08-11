using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Tilework.LoadBalancing.Persistence;

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
        await using var scope = _serviceProvider.CreateAsyncScope();

        _logger.LogInformation($"Running migrations for context: LoadBalancerContext");
        var dbContext = scope.ServiceProvider.GetRequiredService<LoadBalancerContext>();
        await dbContext.Database.MigrateAsync(ct);

        var loadBalancerService = scope.ServiceProvider.GetRequiredService<LoadBalancerService>();
        await loadBalancerService.ApplyConfiguration();
    }

    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
}