using Tilework.LoadBalancing.Services;

namespace Tilework.Ui.ViewModels;

public class LoadBalancerListViewModel
{

    private readonly LoadBalancerService _loadBalancerService;

    public List<BaseLoadBalancer> Balancers { get; set; } = new List<BaseLoadBalancer>();

    public LoadBalancerListViewModel(LoadBalancerService loadBalancerService)
    {
        _loadBalancerService = loadBalancerService;
    }

    public async Task Initialize()
    {
        Balancers = await _loadBalancerService.GetLoadBalancers();
    }
}