using Tilework.LoadBalancing.Enums;

namespace Tilework.LoadBalancing.Persistence.Models;

public class Condition
{
    public ConditionType Type { get; set; }

    public string Value { get; set; }

    public ConditionOperator? Operator { get; set; }
}

