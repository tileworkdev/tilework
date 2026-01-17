using System.ComponentModel;

namespace Tilework.LoadBalancing.Enums;

public enum LoadBalancerProtocol
{
    [Description("HTTP")]
    HTTP,
    [Description("HTTPS")]
    HTTPS,
    [Description("TCP")]
    TCP,
    [Description("UDP")]
    UDP,
    [Description("TCP/UDP")]
    TCP_UDP,
    [Description("TLS")]
    TLS
}