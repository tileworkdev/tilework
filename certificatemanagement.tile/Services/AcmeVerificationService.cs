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

    private async Task<Container> CreateContainer(string filename, string fileData)
    {
        var container = await _containerManager.CreateContainer(
            $"AcmeVerification-{filename}",
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
                $"/usr/share/nginx/html/.well-known/acme-challenge/{filename}"
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

    private async Task DeleteContainer(string filename)
    {
        var containers = await _containerManager.ListContainers("certificatemanagement.tile");
        var container = containers.First(cnt => cnt.Name == $"AcmeVerification-{filename}");

        if (container.State == ContainerState.Running)
            await _containerManager.StopContainer(container.Id);
        await _containerManager.DeleteContainer(container.Id);
    }

    private async Task<ApplicationLoadBalancer> AddLoadBalancer()
    {
        var lb = new ApplicationLoadBalancer()
        {
            Name = "AcmeVerification",
            Protocol = AlbProtocol.HTTP,
            Port = 80,
            Enabled = true
        };
        await _loadBalancerService.AddLoadBalancer(lb);
        return lb;
    }

    private async Task AddLoadBalancerTarget(ApplicationLoadBalancer balancer, string host, string filename)
    {
        var tg = new TargetGroup()
        {
            Name = $"AcmeVerification-{filename}",
            Protocol = TargetGroupProtocol.HTTP
        };

        await _loadBalancerService.AddTargetGroup(tg);

        var lowestPriority = balancer.Rules
            .Select(r => r.Priority)
            .DefaultIfEmpty(0)
            .Min();

        var rule = new Rule()
        {
            Listener = balancer,
            TargetGroup = tg,
            Priority = lowestPriority <= 0 ? lowestPriority - 1 : -1,
            Conditions = new List<Condition>()
            {
                new Condition() {
                    Type = ConditionType.HostHeader,
                    Values = new List<string>() {
                        host
                    }
                },
                new Condition() {
                    Type = ConditionType.Path,
                    Values = new List<string>() {
                        $"/.well-known/acme-challenge/{filename}"
                    }
                }
            }
        };

        balancer.Rules.Add(rule);
        await _loadBalancerService.UpdateLoadBalancer(balancer);
    }

    private async Task CheckRemoveLoadBalancer()
    {

    }

    public async Task StartVerification(string host, string filename, string data)
    {
        _logger.LogInformation($"Starting HTTP-01 verification server for {host}/{filename}");

        var balancers = await _loadBalancerService.GetLoadBalancers();

        var balancer = balancers.FirstOrDefault(lb => lb.Port == 80 && lb.Enabled == true);

        if (balancer == null)
        {
            _logger.LogInformation($"No existing load balancer found in port 80. Adding temporary load balancer for ACME verification");
            balancer = await AddLoadBalancer();
        }
        else
        {
            if (balancer is not ApplicationLoadBalancer)
            {
                throw new InvalidOperationException("Cannot run verification. A network load balancer uses port 80");
            }

            _logger.LogInformation($"Existing load balancer found in port 80. Adding temporary rule for ACME verification");
        }

        var container = await CreateContainer(filename, data);


        await AddLoadBalancerTarget((ApplicationLoadBalancer)balancer, host, filename);
        await _loadBalancerService.ApplyConfiguration();

    }

    public async Task StopVerification(string host, string filename)
    {
        _logger.LogInformation($"Stopping HTTP-01 verification server for {host}/{filename}");
        await DeleteContainer(filename);
        await CheckRemoveLoadBalancer();
    }

    public async Task StopAllVerifications()
    {
        var containers = (await _containerManager.ListContainers("certificatemanagement.tile"))
            .Where(cnt => cnt.Name.StartsWith("AcmeVerification-"))
            .ToList();

        foreach (var container in containers)
        {
            var filename = container.Name.Substring("AcmeVerification-".Length);
            await DeleteContainer(filename);
            await CheckRemoveLoadBalancer();
        }
        
    }
}