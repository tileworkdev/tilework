using Tilework.LoadBalancing.Enums;

namespace Tilework.LoadBalancing.Models;


public class NetworkLoadBalancerDTO : BaseLoadBalancerDTO
{
    public NlbProtocol Protocol { get; set; }
    public Guid TargetGroup { get; set; }
}