using System.ComponentModel;

namespace Tilework.LoadBalancing.Enums;

public enum LoadBalancerType
{
    [Description("Application")]
    APPLICATION,
    [Description("Network")]
    NETWORK
}