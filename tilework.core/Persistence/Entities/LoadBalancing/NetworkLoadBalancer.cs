using Tilework.LoadBalancing.Enums;

namespace Tilework.Persistence.LoadBalancing.Models;

public class NetworkLoadBalancer : BaseLoadBalancer
{
    public NlbProtocol Protocol { get; set; }

    public Guid TargetGroupId { get; set; }
    public virtual TargetGroup TargetGroup { get; set; }
}