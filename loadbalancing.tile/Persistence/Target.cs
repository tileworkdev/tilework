using System.Net;

namespace Tilework.LoadBalancing.Persistence.Models;


public class Target
{
    public Guid Id { get; set; }
    public IPAddress Address { get; set; }
    public int Port { get; set; }

    public Guid TargetGroupId { get; set; }
    public virtual TargetGroup TargetGroup { get; set; }
}