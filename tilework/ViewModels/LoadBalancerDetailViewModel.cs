using Tilework.LoadBalancing.Models;
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
        _loadBalancerService.UpdateLoadBalancer(Object);
        _loadBalancerService.ApplyConfiguration();
    }
    
    public async Task Disable()
    {
        Object.Enabled = false;
        _loadBalancerService.UpdateLoadBalancer(Object);
        _loadBalancerService.ApplyConfiguration();
    }

    public async Task Delete()
    {
        _loadBalancerService.DeleteLoadBalancer(Object.Id);
        _loadBalancerService.ApplyConfiguration();
    }

    public async Task Initialize(Guid Id)
    {
        Object = _loadBalancerService.GetLoadBalancer(Id);
    }
}