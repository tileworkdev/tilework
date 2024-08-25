using Tilework.LoadBalancing.Models;
using Tilework.LoadBalancing.Services;

namespace Tilework.ViewModels;

public class LoadBalancerNewViewModel
{

    private readonly LoadBalancerService _loadBalancerService;

    public LoadBalancer Object;

    public LoadBalancerNewViewModel(LoadBalancerService loadBalancerService)
    {
        _loadBalancerService = loadBalancerService;
    }

    public async Task Initialize()
    {
        Object = new LoadBalancer()
        {
            Id=Guid.NewGuid(),
            Enabled=true
        };
    }

    public async Task Save()
    {
        _loadBalancerService.AddLoadBalancer(Object);
        _loadBalancerService.ApplyConfiguration();
    }

    public async Task<List<TargetGroup>> GetTargetGroups()
    {
        return _loadBalancerService.GetTargetGroups();
    }
}