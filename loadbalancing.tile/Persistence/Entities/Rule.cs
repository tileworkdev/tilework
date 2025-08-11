namespace Tilework.LoadBalancing.Persistence.Models;


public class Rule
{
    public Guid Id { get; set; }

    public Guid TargetGroupId { get; set; }
    public virtual TargetGroup TargetGroup { get; set; }

    public Guid ListenerId { get; set; }
    public virtual ApplicationLoadBalancer Listener { get; set; }

    public List<Condition> Conditions { get; set; } = new();
}