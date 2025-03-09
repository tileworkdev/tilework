using Tilework.LoadBalancing.Persistence.Models;
using Tilework.LoadBalancing.Services;

namespace Tilework.LoadBalancing.ViewModels;

public class TargetGroupNewViewModel
{

    private readonly LoadBalancerService _loadBalancerService;

    public TargetGroup Object;

    public TargetGroupNewViewModel(LoadBalancerService loadBalancerService)
    {
        _loadBalancerService = loadBalancerService;
    }

    public async Task Initialize()
    {
        Object = new TargetGroup();
    }

    public async Task Save()
    {
        await _loadBalancerService.AddTargetGroup(Object);
    }
}