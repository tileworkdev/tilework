using Tilework.LoadBalancing.Persistence.Models;
using Tilework.LoadBalancing.Services;

namespace Tilework.ViewModels;

public class TargetGroupListViewModel
{
    private readonly LoadBalancerService _loadBalancerService;

    public List<TargetGroup> TargetGroups { get; set; } = new List<TargetGroup>();

    public TargetGroupListViewModel(LoadBalancerService loadBalancerService)
    {
        _loadBalancerService = loadBalancerService;
    }

    public async Task Initialize()
    {
        TargetGroups = await _loadBalancerService.GetTargetGroups();
    }
}