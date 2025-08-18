using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Tilework.Core.Interfaces;
using Tilework.Core.Models;
using Tilework.Core.Enums;

using Tilework.CertificateManagement.Models;

using Tilework.LoadBalancing.Enums;
using Tilework.LoadBalancing.Models;
using Tilework.LoadBalancing.Interfaces;


namespace Tilework.CertificateManagement.Services;

public class AcmeVerificationService
{
    private readonly ILogger<AcmeVerificationService> _logger;
    private readonly ILoadBalancerService _loadBalancerService;
    private readonly CertificateManagementConfiguration _settings;
    private readonly IContainerManager _containerManager;


    public AcmeVerificationService(ILogger<AcmeVerificationService> logger,
                                   ILoadBalancerService loadBalancerService,
                                   IOptions<CertificateManagementConfiguration> settings,
                                   IContainerManager containerManager)
    {
        _logger = logger;
        _loadBalancerService = loadBalancerService;
        _settings = settings.Value;
        _containerManager = containerManager;
    }

    private async Task<Container> CreateContainer(string name, string filename, string fileData)
    {
        var container = await _containerManager.CreateContainer(
            $"AcmeVerification-{name}",
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

    private async Task DeleteContainer(string name)
    {
        var containers = await _containerManager.ListContainers("certificatemanagement.tile");
        var container = containers.First(cnt => cnt.Name == $"AcmeVerification-{name}");

        if (container.State == ContainerState.Running)
            await _containerManager.StopContainer(container.Id);
        await _containerManager.DeleteContainer(container.Id);
    }

    private async Task<ApplicationLoadBalancerDTO> AddLoadBalancer()
    {
        var lb = new ApplicationLoadBalancerDTO()
        {
            Name = "AcmeVerification",
            Protocol = AlbProtocol.HTTP,
            Port = 80
        };
        lb = (ApplicationLoadBalancerDTO) await _loadBalancerService.AddLoadBalancer(lb);
        await _loadBalancerService.EnableLoadBalancer(lb.Id);
        return lb;
    }

    private async Task AddLoadBalancerTarget(string id, ApplicationLoadBalancerDTO balancer, string host, string filename, string target)
    {
        var tg = new TargetGroupDTO()
        {
            Name = $"AcmeVerification-{id}",
            Protocol = TargetGroupProtocol.HTTP,
        };

        tg = await _loadBalancerService.AddTargetGroup(tg);

        var t = new TargetDTO()
        {
            Host = Host.Parse(target),
            Port = 80,
        };

        await _loadBalancerService.AddTarget(tg, t);



        var lowestPriority = (await _loadBalancerService.GetRules(balancer))
            .Select(r => r.Priority)
            .DefaultIfEmpty(0)
            .Min();

        var rule = new RuleDTO()
        {
            TargetGroup = tg.Id,
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

        await _loadBalancerService.AddRule(balancer, rule);
    }

    private async Task CheckRemoveLoadBalancer(string certId)
    {
        var balancers = await _loadBalancerService.GetLoadBalancers();
        var balancer = balancers.FirstOrDefault(lb => lb.Port == 80);

        if (balancer == null)
            return;

        var appBalancer = (ApplicationLoadBalancerDTO)balancer;

        var targetGroups = await _loadBalancerService.GetTargetGroups();

        foreach (var rule in await _loadBalancerService.GetRules(appBalancer))
        {
            var tg = targetGroups.FirstOrDefault(tg => tg.Id == rule.TargetGroup);

            if (tg != null && tg.Name == $"AcmeVerification-{certId}")
            {
                await _loadBalancerService.RemoveRule(appBalancer, rule);
                await _loadBalancerService.DeleteTargetGroup(tg.Id);
            }

        }

        if (appBalancer.Name == "AcmeVerification" && (await _loadBalancerService.GetRules(appBalancer)).Count == 0)
        {
            await _loadBalancerService.DeleteLoadBalancer(appBalancer.Id);
        }

        await _loadBalancerService.ApplyConfiguration();
    }

    public async Task StartVerification(string id, string host, string filename, string data)
    {
        _logger.LogInformation($"Starting HTTP-01 verification server for {host}/{id}");

        var balancers = await _loadBalancerService.GetLoadBalancers();

        var balancer = balancers.FirstOrDefault(lb => lb.Port == 80 && lb.Enabled == true);

        if (balancer == null)
        {
            _logger.LogInformation($"No existing load balancer found in port 80. Adding temporary load balancer for ACME verification");
            balancer = await AddLoadBalancer();
        }
        else
        {
            if (balancer is not ApplicationLoadBalancerDTO)
            {
                throw new InvalidOperationException("Cannot run verification. A network load balancer uses port 80");
            }

            _logger.LogInformation($"Existing load balancer found in port 80. Adding temporary rule for ACME verification");
        }

        var container = await CreateContainer(id, filename, data);


        await AddLoadBalancerTarget(id, (ApplicationLoadBalancerDTO)balancer, host, filename, container.Name);
        await _loadBalancerService.ApplyConfiguration();
    }

    public async Task StopVerification(string id)
    {
        _logger.LogInformation($"Stopping HTTP-01 verification server for {id}");
        await DeleteContainer(id);
        await CheckRemoveLoadBalancer(id);
    }

    public async Task StopAllVerifications()
    {
        var containers = (await _containerManager.ListContainers("certificatemanagement.tile"))
            .Where(cnt => cnt.Name.StartsWith("AcmeVerification-"))
            .ToList();

        foreach (var container in containers)
        {
            var name = container.Name.Substring("AcmeVerification-".Length);
            await DeleteContainer(name);
            await CheckRemoveLoadBalancer(name);
        }
    }
}