using System.Reflection;
using Tilework.Core.Attributes;

namespace Tilework.LoadBalancing.Models;

public class LoadBalancingMonitorData : BaseMonitorData
{
    public int Sessions { get; set; } // stot
    public int Requests { get; set; } // req_tot
}