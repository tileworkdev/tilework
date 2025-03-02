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

    public async Task AddListener(ApplicationListener listener)
    {
        Object.ApplicationListeners.Add(listener);
        await _loadBalancerService.UpdateLoadBalancer(Object);
        await _loadBalancerService.ApplyConfiguration();
    }

    public async Task AddListener(NetworkListener listener)
    {
        Object.NetworkListeners.Add(listener);
        await _loadBalancerService.UpdateLoadBalancer(Object);
        await _loadBalancerService.ApplyConfiguration();
    }

    public async Task DeleteListener(ApplicationListener listener)
    {
        Object.ApplicationListeners.Remove(listener);
        await _loadBalancerService.UpdateLoadBalancer(Object);
        await _loadBalancerService.ApplyConfiguration();
    }

    public async Task DeleteListener(NetworkListener listener)
    {
        Object.NetworkListeners.Remove(listener);
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