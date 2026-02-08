using Tilework.Core.Models;

namespace Tilework.LoadBalancing.Models;


public class RuleDTO
{
    public Guid Id { get; set; }
    public int Priority { get; set; }
    public Guid LoadBalancer { get; set; }
    public Guid? TargetGroup { get; set; }
    public RuleAction Action { get; set; } = new();

    public List<Condition> Conditions { get; set; } = new();
}
