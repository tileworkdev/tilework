using Tilework.LoadBalancing.Enums;

namespace Tilework.LoadBalancing.Models;

public class RuleAction
{
    public RuleActionType Type { get; set; } = RuleActionType.Forward;

    public string? RedirectUrl { get; set; }
    public int? RedirectStatusCode { get; set; }

    public int? FixedResponseStatusCode { get; set; }
    public string? FixedResponseBody { get; set; }
    public string? FixedResponseContentType { get; set; }
}
