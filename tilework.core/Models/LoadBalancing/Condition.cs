using Tilework.LoadBalancing.Enums;

namespace Tilework.LoadBalancing.Models;

public class Condition
{
    public ConditionType Type { get; set; }

    public List<string> Values { get; set; } = new List<string>();
}