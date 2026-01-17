using System.ComponentModel;

namespace Tilework.LoadBalancing.Enums;

public enum ConditionType
{
    [Description("Host header")]
    HostHeader,
    [Description("Path")]
    Path,
    [Description("Query string")]
    QueryString,
    [Description("SNI FQDN")]
    SNI
}

