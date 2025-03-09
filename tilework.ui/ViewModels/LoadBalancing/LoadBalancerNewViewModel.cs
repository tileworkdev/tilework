using Tilework.LoadBalancing.Persistence.Models;
using Tilework.LoadBalancing.Services;
using Tilework.LoadBalancing.Enums;

namespace Tilework.Ui.ViewModels;

public class LoadBalancerNewViewModel
{

    private readonly LoadBalancerService _loadBalancerService;

    public BaseLoadBalancer Object;

    private LoadBalancerType _loadBalancerType;
    public LoadBalancerType LoadBalancerType
    {
        get { return _loadBalancerType; }
        set
        {
            if(_loadBalancerType == LoadBalancerType.APPLICATION && value == LoadBalancerType.NETWORK)
            {
                Object = new NetworkLoadBalancer()
                {
                    Id=Object.Id,
                    Name=Object.Name,
                    Port=Object.Port,
                    Enabled=Object.Enabled
                };
            }
            else if(_loadBalancerType == LoadBalancerType.NETWORK && value == LoadBalancerType.APPLICATION)
            {
                Object = new ApplicationLoadBalancer()
                {
                    Id=Object.Id,
                    Name=Object.Name,
                    Port=Object.Port,
                    Enabled=Object.Enabled
                };
            }

            _loadBalancerType = value;
        }
    }

    public LoadBalancerNewViewModel(LoadBalancerService loadBalancerService)
    {
        _loadBalancerService = loadBalancerService;
    }

    public async Task Initialize()
    {
        Object = new ApplicationLoadBalancer()
        {
            Id=Guid.NewGuid(),
            Enabled=true
        };
    }

    public async Task Save()
    {
        await _loadBalancerService.AddLoadBalancer(Object);
        await _loadBalancerService.ApplyConfiguration();
    }

    public async Task<List<TargetGroup>> GetNlbTargetGroups()
    {
        var protocols = new List<TargetGroupProtocol> {
            TargetGroupProtocol.TCP,
            TargetGroupProtocol.UDP,
            TargetGroupProtocol.TCP_UDP,
            TargetGroupProtocol.TLS
        };
        return (await _loadBalancerService.GetTargetGroups()).Where(tg => protocols.Contains(tg.Protocol)).ToList();
    }
}