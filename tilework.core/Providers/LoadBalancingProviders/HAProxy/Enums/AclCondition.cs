using System.ComponentModel;

namespace Tilework.LoadBalancing.Haproxy;

public enum AclCondition
{
    HostHeader,
    Path,
    QueryString,
    SNI,
    SourceIp,
    VariableSet,
}