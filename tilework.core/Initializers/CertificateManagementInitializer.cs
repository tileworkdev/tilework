using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;

using Microsoft.Extensions.DependencyInjection;



namespace Tilework.CertificateManagement.Services;
public sealed class CertificateManagementInitializer : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CertificateManagementInitializer> _logger;

    public CertificateManagementInitializer(ILogger<CertificateManagementInitializer> logger,
                                            IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public Task StartAsync(CancellationToken ct) => Task.CompletedTask;

    public async Task StopAsync(CancellationToken ct)
    {
        _logger.LogInformation($"Initiating shutdown for module: LoadBalancing");
        await using var scope = _serviceProvider.CreateAsyncScope();
        var loadBalancerService = scope.ServiceProvider.GetRequiredService<AcmeVerificationService>();

        await loadBalancerService.StopAllVerifications();
    }
}