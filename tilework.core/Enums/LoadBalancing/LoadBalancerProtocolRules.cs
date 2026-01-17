using System.Linq;

namespace Tilework.LoadBalancing.Enums;

public static class LoadBalancerProtocolRules
{
    private static readonly LoadBalancerProtocol[] ApplicationProtocols =
    {
        LoadBalancerProtocol.HTTP,
        LoadBalancerProtocol.HTTPS
    };

    private static readonly LoadBalancerProtocol[] NetworkProtocols =
    {
        LoadBalancerProtocol.TCP,
        LoadBalancerProtocol.UDP,
        LoadBalancerProtocol.TCP_UDP,
        LoadBalancerProtocol.TLS
    };

    public static IReadOnlyList<LoadBalancerProtocol> GetAllowedProtocols(LoadBalancerType type)
    {
        return type == LoadBalancerType.NETWORK ? NetworkProtocols : ApplicationProtocols;
    }

    public static bool IsAllowed(LoadBalancerType type, LoadBalancerProtocol protocol)
    {
        return GetAllowedProtocols(type).Contains(protocol);
    }
}
