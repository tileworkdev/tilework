using Tilework.Monitoring.Models;
using Tilework.LoadBalancing.Enums;

namespace Tilework.LoadBalancing.Models;

public class LoadBalancingStatusData : BaseMonitorData
{
    public LoadBalancerStatus Status { get; set; }
}
