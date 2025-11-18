using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

using Coravel;
using Tilework.Core.Jobs.CertificateManagement;


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

    public async Task StartAsync(CancellationToken ct)
    {
        _logger.LogInformation($"Initiating startup for module: CertificateManagement");
        await using var scope = _serviceProvider.CreateAsyncScope();

        scope.ServiceProvider.UseScheduler(s =>
        {
            s.Schedule<CertificateRenewalJob>()
             .Hourly()
             .PreventOverlapping("CertificateRenewalJob");
        });   
    }

    public async Task StopAsync(CancellationToken ct)
    {
        _logger.LogInformation($"Initiating shutdown for module: CertificateManagement");
        await using var scope = _serviceProvider.CreateAsyncScope();
        var loadBalancerService = scope.ServiceProvider.GetRequiredService<AcmeVerificationService>();

        await loadBalancerService.StopAllVerifications();
    }
}