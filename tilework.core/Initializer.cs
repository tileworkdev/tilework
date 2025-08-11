using Microsoft.Extensions.Hosting;

namespace Tilework.Core.Services;

public sealed class CoreInitializer : IHostedService
{
    public Task StartAsync(CancellationToken ct) => Task.CompletedTask;
    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
}