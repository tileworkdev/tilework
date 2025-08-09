using Tilework.LoadBalancing.Services;
using Tilework.Ui.Interfaces;

namespace Tilework.Ui.ViewModels;

public class LoadBalancerListViewModel : IListViewModel<BaseLoadBalancer>
{

    private readonly LoadBalancerService _loadBalancerService;

    public List<BaseLoadBalancer> Items { get; set; } = new List<BaseLoadBalancer>();

    public LoadBalancerListViewModel(LoadBalancerService loadBalancerService)
    {
        _loadBalancerService = loadBalancerService;
    }

    public async Task Initialize()
    {
        Items = await _loadBalancerService.GetLoadBalancers();
    }
}