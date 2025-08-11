using Microsoft.Extensions.Logging;


using Tilework.Core.Interfaces;
using Tilework.LoadBalancing.Services;
using Tilework.CertificateManagement.Persistence;
using Tilework.CertificateManagement.Settings;
using Tilework.LoadBalancing.Persistence.Models;


namespace Tilework.CertificateManagement.Services;

public class AcmeVerificationService
{
    private readonly ILogger<CertificateManagementService> _logger;
    private readonly LoadBalancerService _loadBalancerService;
    private readonly IContainerManager _containerManager;


    public AcmeVerificationService(ILogger<CertificateManagementService> logger,
                                   LoadBalancerService loadBalancerService,
                                   IContainerManager containerManager)
    {
        _logger = logger;
        _loadBalancerService = loadBalancerService;
        _containerManager = containerManager;
    }

    public async Task StartVerification()
    {
        var lbs = await _loadBalancerService.GetLoadBalancers();

        var lb = lbs.FirstOrDefault(lb => lb.Port == 80);

        if (lb == null)
        {

        }
        else
        {
            if (lb is not ApplicationLoadBalancer)
            {
                throw new InvalidOperationException("Cannot run verification. A network load balancer uses port 80");
            }
        }

    }

    public async Task StopVerification()
    {

    }

    public async Task StartVerificationContainer()
    {

    }

    public async Task StopVerificationContainer()
    {

    }
}