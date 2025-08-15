using Tilework.Core.Models;

namespace Tilework.Core.LoadBalancing.Models;


public class RuleDTO
{
    public Guid Id { get; set; }
    public int Priority { get; set; }

    public List<Condition> Conditions { get; set; } = new();
    public Guid TargetGroup { get; set; }
}