using Tilework.Core.LoadBalancing.Enums;

namespace Tilework.Core.LoadBalancing.Models;


public class ApplicationLoadBalancerDTO : BaseLoadBalancerDTO
{
    public AlbProtocol Protocol { get; set; }
}