using Tilework.LoadBalancing.Persistence.Models;
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
        await _loadBalancerService.UpdateLoadBalancer(Object);
        await _loadBalancerService.ApplyConfiguration();
    }

    public async Task Initialize(Guid Id)
    {
        var obj = await _loadBalancerService.GetLoadBalancer(Id);
        if(obj == null)
            throw new KeyNotFoundException();
        Object = obj;
    }

    public async Task<List<TargetGroup>> GetTargetGroups()
    {
        return await _loadBalancerService.GetTargetGroups();
    }
}