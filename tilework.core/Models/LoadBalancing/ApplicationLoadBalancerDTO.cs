using Tilework.LoadBalancing.Enums;

namespace Tilework.LoadBalancing.Models;


public class ApplicationLoadBalancerDTO : BaseLoadBalancerDTO
{
    public AlbProtocol Protocol { get; set; }
}