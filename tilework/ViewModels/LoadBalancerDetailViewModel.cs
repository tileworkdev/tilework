using Tilework.LoadBalancing.Persistence.Models;
using Tilework.LoadBalancing.Services;

namespace Tilework.ViewModels;

public class LoadBalancerDetailViewModel
{

    private readonly LoadBalancerService _loadBalancerService;

    public LoadBalancer Object;

    public LoadBalancerDetailViewModel(LoadBalancerService loadBalancerService)
    {
        _loadBalancerService = loadBalancerService;
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

    public async Task Delete()
    {
        await _loadBalancerService.DeleteLoadBalancer(Object);
        await _loadBalancerService.ApplyConfiguration();
    }

    public async Task Initialize(Guid Id)
    {
        Object = await _loadBalancerService.GetLoadBalancer(Id);
    }
}