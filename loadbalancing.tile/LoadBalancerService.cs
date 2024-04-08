using Tilework.LoadBalancing.Models;
using Tilework.LoadBalancing.Enums;

namespace Tilework.LoadBalancing.Services;


public class LoadBalancerService
{
    private List<LoadBalancer> balancers = new List<LoadBalancer>
    {
        new LoadBalancer { Id = Guid.Parse("e8d5f029-4996-4bc1-86ec-5a92f0194b49"), Name = "LB1", Type = LoadBalancerType.NETWORK, Port = 8080, Group = null },
        new LoadBalancer { Id = Guid.Parse("a550a4bd-2330-45b6-800f-345c1ffb5ff7"), Name = "LB2", Type = LoadBalancerType.APPLICATION, Port = 443, Group = null },
        new LoadBalancer { Id = Guid.Parse("a98a013d-31de-4a43-b990-c49658d3ed38"), Name = "LB3", Type = LoadBalancerType.NETWORK, Port = 53, Group = null }
    };

    public List<LoadBalancer> GetLoadBalancers()
    {
        return balancers;
    }

    public LoadBalancer GetLoadBalancer(Guid Id)
    {
        return balancers.First(b => b.Id == Id);
    }
}
