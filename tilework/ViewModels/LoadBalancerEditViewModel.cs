using Tilework.LoadBalancing.Models;
using Tilework.LoadBalancing.Services;

namespace Tilework.ViewModels;

public class LoadBalancerEditViewModel
{

    private readonly LoadBalancerService _loadBalancerService;

    public LoadBalancer Object;

    public LoadBalancerEditViewModel(LoadBalancerService loadBalancerService)
    {
        _loadBalancerService = loadBalancerService;
    }

    public async Task Enable()
    {
        Object.Enabled = true;
        await Save();
    }
    
    public async Task Disable()
    {
        Object.Enabled = false;
        await Save();
    }

    public async Task Save()
    {
        _loadBalancerService.UpdateLoadBalancer(Object);
        _loadBalancerService.ApplyConfiguration();
    }

    public async Task Initialize(Guid Id)
    {
        Object = _loadBalancerService.GetLoadBalancer(Id);
    }

    public async Task<List<TargetGroup>> GetTargetGroups()
    {
        return _loadBalancerService.GetTargetGroups();
    }
}