using System.Reflection;
using Tilework.Core.Attributes;

namespace Tilework.LoadBalancing.Models;

public class LoadBalancingMonitorData : BaseMonitorData
{
    public int Sessions { get; set; }
    public int Requests { get; set; }
    public int HttpResponses1xx { get; set; }
    public int HttpResponses2xx { get; set; }
    public int HttpResponses3xx { get; set; }
    public int HttpResponses4xx { get; set; }
    public int HttpResponses5xx { get; set; }
    public int HttpResponsesOther { get; set; }
}