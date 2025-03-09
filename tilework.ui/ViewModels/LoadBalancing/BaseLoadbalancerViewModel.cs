using Tilework.LoadBalancing.Persistence.Models;
using Tilework.LoadBalancing.Services;

namespace Tilework.Ui.ViewModels;

public class BaseLoadBalancerViewModel
{
    protected readonly LoadBalancerService _loadBalancerService;
    public BaseLoadBalancer Object;

    public BaseLoadBalancerViewModel(LoadBalancerService loadBalancerService)
    {
        _loadBalancerService = loadBalancerService;
    }

    public async Task Initialize(Guid Id)
    {
        var obj = await _loadBalancerService.GetLoadBalancer(Id);
        if(obj == null)
            throw new KeyNotFoundException();
        Object = obj;
    }

    public bool IsAlb()
    {
        return Object is ApplicationLoadBalancer;
    }

    public bool IsNlb()
    {
        return Object is NetworkLoadBalancer;
    }

    public async Task Enable()
    {
        Object.Enabled = true;
        await _loadBalancerService.UpdateLoadBalancer(Object);
        await _loadBalancerService.ApplyConfiguration();
    }
    
    public async Task Disable()
    {
        Object.Enabled = false;
        await _loadBalancerService.UpdateLoadBalancer(Object);
        await _loadBalancerService.ApplyConfiguration();
    }

    public async Task<List<TargetGroup>> GetTargetGroups()
    {
        return await _loadBalancerService.GetTargetGroups();
    }
}