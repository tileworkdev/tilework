using Tilework.LoadBalancing.Persistence.Models;
using Tilework.LoadBalancing.Services;
using Tilework.Ui.Interfaces;
namespace Tilework.Ui.ViewModels;

public class TargetGroupListViewModel : IListViewModel<TargetGroup>
{
    private readonly LoadBalancerService _loadBalancerService;

    public List<TargetGroup> Items { get; set; } = new List<TargetGroup>();

    public TargetGroupListViewModel(LoadBalancerService loadBalancerService)
    {
        _loadBalancerService = loadBalancerService;
    }

    public async Task Initialize()
    {
        Items = await _loadBalancerService.GetTargetGroups();
    }
}