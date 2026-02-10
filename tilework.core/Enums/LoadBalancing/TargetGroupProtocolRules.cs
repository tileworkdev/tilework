using System.Collections.Generic;
using System.Linq;

namespace Tilework.LoadBalancing.Enums;

public static class TargetGroupProtocolRules
{
    private static readonly TargetGroupProtocol[] ApplicationProtocols =
    {
        TargetGroupProtocol.HTTP,
        TargetGroupProtocol.HTTPS
    };

    private static readonly TargetGroupProtocol[] NetworkProtocols =
    {
        TargetGroupProtocol.TCP,
        // TargetGroupProtocol.UDP,
        // TargetGroupProtocol.TCP_UDP,
        TargetGroupProtocol.TLS
    };

    public static IReadOnlyList<TargetGroupProtocol> GetAllowedProtocols(LoadBalancerType type)
    {
        return type == LoadBalancerType.NETWORK ? NetworkProtocols : ApplicationProtocols;
    }

    public static IReadOnlyList<TargetGroupProtocol> GetAllowedProtocols()
    {
        return ApplicationProtocols
            .Concat(NetworkProtocols)
            .Distinct()
            .ToList();
    }

    public static bool IsAllowed(LoadBalancerType type, TargetGroupProtocol protocol)
    {
        return GetAllowedProtocols(type).Contains(protocol);
    }
}
