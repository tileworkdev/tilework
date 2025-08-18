using System.ComponentModel;

namespace Tilework.LoadBalancing.Enums;

public enum TargetGroupProtocol
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