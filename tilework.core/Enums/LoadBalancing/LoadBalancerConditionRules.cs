using System;
using System.Linq;

namespace Tilework.LoadBalancing.Enums;

public static class LoadBalancerConditionRules
{
    private static readonly ConditionType[] HttpConditions =
    {
        ConditionType.HostHeader,
        ConditionType.Path,
        ConditionType.QueryString
    };

    private static readonly ConditionType[] HttpsConditions =
    {
        ConditionType.HostHeader,
        ConditionType.Path,
        ConditionType.QueryString,
        ConditionType.SNI
    };

    private static readonly ConditionType[] TlsConditions =
    {
        ConditionType.SNI
    };

    public static IReadOnlyList<ConditionType> GetAllowedConditions(LoadBalancerProtocol protocol)
    {
        return protocol switch
        {
            LoadBalancerProtocol.HTTP => HttpConditions,
            LoadBalancerProtocol.HTTPS => HttpsConditions,
            LoadBalancerProtocol.TLS => TlsConditions,
            _ => Array.Empty<ConditionType>()
        };
    }

    public static bool IsAllowed(LoadBalancerProtocol protocol, ConditionType condition)
    {
        return GetAllowedConditions(protocol).Contains(condition);
    }
}
