using System.Reflection;
using Tilework.Core.Attributes;
using Tilework.LoadBalancing.Enums;

namespace Tilework.LoadBalancing.Models;

public class LoadBalancingStatusData : BaseMonitorData
{
    public LoadBalancerStatus Status { get; set; }
}
