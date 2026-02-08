using System.ComponentModel;

namespace Tilework.LoadBalancing.Enums;

public enum RuleActionType
{
    [Description("Forward to target group")]
    Forward,
    [Description("HTTP redirect")]
    Redirect,
    [Description("HTTP fixed response")]
    FixedResponse,
    [Description("Reject connection")]
    Reject
}
