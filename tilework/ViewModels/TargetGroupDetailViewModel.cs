using Tilework.LoadBalancing.Models;
using Tilework.LoadBalancing.Services;

namespace Tilework.ViewModels;

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
        Object = _loadBalancerService.GetTargetGroup(Id);
    }
}