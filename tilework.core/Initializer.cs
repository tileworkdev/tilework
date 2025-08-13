using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

using Tilework.Core.Interfaces;
namespace Tilework.Core.Services;

public sealed class CoreInitializer : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CoreInitializer> _logger;

    public CoreInitializer(ILogger<CoreInitializer> logger,
                           IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public async Task StartAsync(CancellationToken ct)
    {
        _logger.LogInformation($"Initiating startup for module: Core");
        await using var scope = _serviceProvider.CreateAsyncScope();
    }


    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
}