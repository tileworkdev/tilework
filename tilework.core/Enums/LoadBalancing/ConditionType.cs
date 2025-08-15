using System.ComponentModel;

namespace Tilework.Core.LoadBalancing.Enums;

public enum ConditionType
{
    [Description("Host header")]
    HostHeader,
    [Description("Path")]
    Path,
    [Description("Query string")]
    QueryString
}

