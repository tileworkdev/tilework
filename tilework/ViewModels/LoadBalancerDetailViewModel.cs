using Tilework.LoadBalancing.Enums;
using Tilework.LoadBalancing.Persistence.Models;
using Tilework.LoadBalancing.Services;

namespace Tilework.ViewModels;

public class LoadBalancerDetailViewModel : BaseLoadBalancerViewModel
{
    public LoadBalancerDetailViewModel(LoadBalancerService loadBalancerService) : base(loadBalancerService)
    {
    }

    public async Task Delete()
    {
        await _loadBalancerService.DeleteLoadBalancer(Object);
        await _loadBalancerService.ApplyConfiguration();
    }

    public async Task<List<TargetGroup>> GetAlbTargetGroups()
    {
        var protocols = new List<TargetGroupProtocol> {
            TargetGroupProtocol.HTTP,
            TargetGroupProtocol.HTTPS
        };
        return (await _loadBalancerService.GetTargetGroups()).Where(tg => protocols.Contains(tg.Protocol)).ToList();
    }

    public async Task AddRule(Rule rule)
    {
        if(Object is ApplicationLoadBalancer appBalancer)
        {
            appBalancer.Rules.Add(rule);
            await _loadBalancerService.UpdateLoadBalancer(appBalancer);
        }
        else
        {
            throw new ArgumentException("Can only add rules to application load balancers");
        }
    }
}