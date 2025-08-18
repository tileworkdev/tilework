using Microsoft.EntityFrameworkCore;
using Tilework.LoadBalancing.Models;


namespace Tilework.Persistence.LoadBalancing.Models;

[Index(nameof(Priority), nameof(LoadBalancerId), IsUnique = true)]
public class Rule
{
    public Guid Id { get; set; }
    public int Priority { get; set; }

    public Guid TargetGroupId { get; set; }
    public virtual TargetGroup TargetGroup { get; set; }

    public Guid LoadBalancerId { get; set; }
    public virtual ApplicationLoadBalancer LoadBalancer { get; set; }

    public List<Condition> Conditions { get; set; } = new();
}