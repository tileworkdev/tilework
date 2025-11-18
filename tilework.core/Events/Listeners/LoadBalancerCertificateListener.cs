using Microsoft.Extensions.Logging;

using Coravel.Events.Interfaces;

using Tilework.LoadBalancing.Interfaces;

namespace Tilework.Events;

public class LoadBalancerCertificateListener : IListener<CertificateRenewed>
{
    private readonly ILoadBalancerService _loadBalancer;
    private readonly ILogger<LoadBalancerCertificateListener> _logger;

    public LoadBalancerCertificateListener(ILoadBalancerService loadBalancer,
                                           ILogger<LoadBalancerCertificateListener> logger)
    {
        _loadBalancer = loadBalancer;
        _logger = logger;
    }

    public async Task HandleAsync(CertificateRenewed evt)
    {
        _logger.LogInformation($"Applying load balancer configuration due to renewal of certificate {evt.Certificate.Name}");
        await _loadBalancer.ApplyConfiguration();
    }
}