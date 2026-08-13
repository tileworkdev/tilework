using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Tilework.Logging.Services;

public sealed class LoggingInitializer : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<LoggingInitializer> _logger;

    public LoggingInitializer(
        IServiceProvider serviceProvider,
        ILogger<LoggingInitializer> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Initiating startup for module: Logging");
        await using var scope = _serviceProvider.CreateAsyncScope();
        var loggingService = scope.ServiceProvider.GetRequiredService<LoggingDataCollectorService>();

        await loggingService.ApplyConfiguration();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Initiating shutdown for module: Logging");
        await using var scope = _serviceProvider.CreateAsyncScope();
        var loggingService = scope.ServiceProvider.GetRequiredService<LoggingDataCollectorService>();

        await loggingService.Shutdown();
    }
}
