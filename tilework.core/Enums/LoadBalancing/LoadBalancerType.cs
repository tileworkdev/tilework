using System.ComponentModel;

namespace Tilework.Core.LoadBalancing.Enums;

public enum LoadBalancerType
{
    [Description("Application")]
    APPLICATION,
    [Description("Network")]
    NETWORK
}