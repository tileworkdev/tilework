using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Tilework.Core.Interfaces;
using Tilework.LoadBalancing.Services;
using Tilework.CertificateManagement.Persistence;
using Tilework.CertificateManagement.Settings;
using Tilework.LoadBalancing.Persistence.Models;
using Tilework.LoadBalancing.Enums;
using Tilework.CertificateManagement.Persistence.Models;
using Tilework.Core.Models;
using Tilework.Core.Enums;

namespace Tilework.CertificateManagement.Services;

public class AcmeVerificationService
{
    private readonly ILogger<CertificateManagementService> _logger;
    private readonly LoadBalancerService _loadBalancerService;
    private readonly CertificateManagementSettings _settings;
    private readonly IContainerManager _containerManager;


    public AcmeVerificationService(ILogger<CertificateManagementService> logger,
                                   LoadBalancerService loadBalancerService,
                                   IOptions<CertificateManagementSettings> settings,
                                   IContainerManager containerManager)
    {
        _logger = logger;
        _loadBalancerService = loadBalancerService;
        _settings = settings.Value;
        _containerManager = containerManager;
    }

    private async Task<Container> CreateContainer(string name, string path, string fileData)
    {
        var container = await _containerManager.CreateContainer(
            name,
            _settings.AcmeVerificationImage,
            "certificatemanagement.tile",
            null
        );

        var tempFilePath = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFilePath, fileData);

            await _containerManager.CopyFileToContainer(
                container.Id,
                tempFilePath,
                $"/usr/share/nginx/html/{path}"
            );
        }
        finally
        {
            if (File.Exists(tempFilePath))
                File.Delete(tempFilePath);
        }

        await _containerManager.StartContainer(container.Id);

        return container;
    }

    private async Task DeleteContainer(string name)
    {
        var containers = await _containerManager.ListContainers("certificatemanagement.tile");
        var container = containers.First(cnt => cnt.Name == name);

        if (container.State == ContainerState.Running)
            await _containerManager.StopContainer(container.Id);
        await _containerManager.DeleteContainer(container.Id);
    }

    private async Task<ApplicationLoadBalancer> AddLoadBalancer()
    {
        var lb = new ApplicationLoadBalancer()
        {
            Name = "AcmeVerification",
            Protocol = AlbProtocol.HTTP
        };
        await _loadBalancerService.AddLoadBalancer(lb);
        return lb;
    }

    public async Task StartVerification(Certificate certificate, string path, string data)
    {
        _logger.LogInformation($"Starting HTTP-01 verification server for certificate {certificate.Name}");
        var container = await CreateContainer(certificate.Id.ToString(), path, data);

        // var lbs = await _loadBalancerService.GetLoadBalancers();

        // var lb = lbs.FirstOrDefault(lb => lb.Port == 80 && lb.Enabled == true);
        // var created = false;

        // if (lb == null)
        // {
        //     lb = await AddLoadBalancer();
        // }
        // else
        // {
        //     if (lb is not ApplicationLoadBalancer)
        //     {
        //         throw new InvalidOperationException("Cannot run verification. A network load balancer uses port 80");
        //     }
        // }

    }

    public async Task StopVerification(Certificate certificate)
    {
        _logger.LogInformation($"Stopping HTTP-01 verification server for certificate {certificate.Name}");
        await DeleteContainer(certificate.Id.ToString());
    }
}