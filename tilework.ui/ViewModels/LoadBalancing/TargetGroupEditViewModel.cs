using Tilework.LoadBalancing.Persistence.Models;
using Tilework.LoadBalancing.Services;

namespace Tilework.Ui.ViewModels;

public class TargetGroupEditViewModel
{

    private readonly LoadBalancerService _loadBalancerService;

    public TargetGroup Object;

    public TargetGroupEditViewModel(LoadBalancerService loadBalancerService)
    {
        _loadBalancerService = loadBalancerService;
    }

    public async Task Save()
    {
        await _loadBalancerService.UpdateTargetGroup(Object);
        await _loadBalancerService.ApplyConfiguration();
    }

    public async Task Initialize(Guid Id)
    {
        var obj = await _loadBalancerService.GetTargetGroup(Id);
        if(obj == null)
            throw new KeyNotFoundException();
        Object = obj;
    }
}