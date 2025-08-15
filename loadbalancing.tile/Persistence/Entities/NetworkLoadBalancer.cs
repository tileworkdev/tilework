using Tilework.Core.LoadBalancing.Enums;

namespace Tilework.LoadBalancing.Persistence.Models;

public class NetworkLoadBalancer : BaseLoadBalancer
{
    public NlbProtocol Protocol { get; set; }

    public Guid TargetGroupId { get; set; }
    public virtual TargetGroup TargetGroup { get; set; }
}