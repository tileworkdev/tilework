using System.ComponentModel;

namespace Tilework.LoadBalancing.Enums;

public enum NlbProtocol
{
    [Description("TCP")]
    TCP,
    [Description("UDP")]
    UDP,
    [Description("TCP/UDP")]
    TCP_UDP,
    [Description("TLS")]
    TLS
}