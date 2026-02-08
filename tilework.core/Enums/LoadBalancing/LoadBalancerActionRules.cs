using System.Collections.Generic;
using System.Linq;

namespace Tilework.LoadBalancing.Enums;

public static class LoadBalancerActionRules
{
    private static readonly RuleActionType[] ApplicationActions =
    {
        RuleActionType.Forward,
        RuleActionType.Redirect,
        RuleActionType.FixedResponse
    };

    private static readonly RuleActionType[] NetworkActions =
    {
        RuleActionType.Forward,
        RuleActionType.Reject
    };

    public static IReadOnlyList<RuleActionType> GetAllowedActions(LoadBalancerType type)
    {
        return type == LoadBalancerType.NETWORK ? NetworkActions : ApplicationActions;
    }

    public static bool IsAllowed(LoadBalancerType type, RuleActionType action)
    {
        return GetAllowedActions(type).Contains(action);
    }
}
