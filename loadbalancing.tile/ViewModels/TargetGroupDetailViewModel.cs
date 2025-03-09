using Tilework.LoadBalancing.Persistence.Models;
using Tilework.LoadBalancing.Services;

namespace Tilework.LoadBalancing.ViewModels;

public class TargetGroupDetailViewModel
{

    private readonly LoadBalancerService _loadBalancerService;

    public TargetGroup Object;

    public TargetGroupDetailViewModel(LoadBalancerService loadBalancerService)
    {
        _loadBalancerService = loadBalancerService;
    }

    public async Task Initialize(Guid Id)
    {
        var obj = await _loadBalancerService.GetTargetGroup(Id);
        if(obj == null)
            throw new KeyNotFoundException();
        Object = obj;
    }

    public async Task Delete()
    {
        await _loadBalancerService.DeleteTargetGroup(Object);
        await _loadBalancerService.ApplyConfiguration();
    }

    public async Task AddTarget(Target target)
    {
        Object.Targets.Add(target);
        await _loadBalancerService.UpdateTargetGroup(Object);
    }
}