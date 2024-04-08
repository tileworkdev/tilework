using Tilework.LoadBalancing.Models;
using Tilework.LoadBalancing.Services;

namespace Tilework.ViewModels;

public class LoadBalancerListViewModel
{

    private readonly LoadBalancerService _loadBalancerService;

    public List<LoadBalancer> Balancers { get; set; } = new List<LoadBalancer>();

    public LoadBalancerListViewModel(LoadBalancerService loadBalancerService)
    {
        _loadBalancerService = loadBalancerService;
    }

    public async Task Initialize()
    {
        Balancers = _loadBalancerService.GetLoadBalancers();
    }
}