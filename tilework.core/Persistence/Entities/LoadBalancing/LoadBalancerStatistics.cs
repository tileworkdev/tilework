using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

using Tilework.LoadBalancing.Models;

namespace Tilework.Persistence.LoadBalancing.Models;

public class LoadBalancerStatistics
{
    public Guid Id { get; set; }

    public Guid LoadBalancerId { get; set; }
    public virtual BaseLoadBalancer LoadBalancer { get; set; }

    public DateTimeOffset Timestamp { get; set; }

    public LoadBalancingStatistics Statistics { get; set; }
}