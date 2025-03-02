using Tilework.LoadBalancing.Enums;

namespace Tilework.LoadBalancing.Persistence.Models;

public class NetworkListener
{
    public Guid Id { get; set; }
    public int Port { get; set; }

    public NlbProtocol Protocol { get; set; }

    public Guid TargetGroupId { get; set; }
    public virtual TargetGroup Group { get; set; }

    public Guid LoadBalancerId { get; set; }
    public virtual LoadBalancer LoadBalancer { get; set; }
}