using Tilework.Core.LoadBalancing.Enums;

namespace Tilework.Core.LoadBalancing.Models;


public class NetworkLoadBalancerDTO : BaseLoadBalancerDTO
{
    public NlbProtocol Protocol { get; set; }
    public Guid TargetGroup { get; set; }
}