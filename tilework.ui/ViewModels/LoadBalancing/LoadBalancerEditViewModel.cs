using Tilework.LoadBalancing.Persistence.Models;
using Tilework.LoadBalancing.Services;

namespace Tilework.Ui.ViewModels;

public class LoadBalancerEditViewModel : BaseLoadBalancerViewModel
{
    public BaseLoadBalancer Object;

    public LoadBalancerEditViewModel(LoadBalancerService loadBalancerService) : base(loadBalancerService)
    {
    }

    public async Task Save()
    {
        await _loadBalancerService.UpdateLoadBalancer(Object);
        await _loadBalancerService.ApplyConfiguration();
    }
}