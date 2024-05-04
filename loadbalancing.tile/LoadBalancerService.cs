using System.Net;

using Tilework.LoadBalancing.Models;
using Tilework.LoadBalancing.Enums;
using Tilework.LoadBalancing.Interfaces;


namespace Tilework.LoadBalancing.Services;


public class LoadBalancerService
{
    private readonly ILoadBalancingConfigurator _configurator;
    
    private static List<TargetGroup> targetGroups = new List<TargetGroup>
    {
        new TargetGroup {
            Id = Guid.Parse("ec45a3e1-b807-4a48-8e27-8f60febb2c39"),
            Name = "ProxyGroup",
            Targets = new List<Target> {
                new Target { Id = Guid.Parse("0209dd00-732b-415a-81cf-13b5d9565938"), Address = IPAddress.Parse("172.16.0.10"), Port=8090 },
                new Target { Id = Guid.Parse("53ca44ae-b44e-4fce-8b75-af0b6121faf2"), Address = IPAddress.Parse("172.16.0.11"), Port=8090 }
            }
        },
        new TargetGroup {
            Id = Guid.Parse("1fa12cdc-72af-44ec-a780-e02157c30b01"),
            Name = "HttpGroup",
            Targets = new List<Target> {
                new Target { Id = Guid.Parse("01eb8280-f554-480e-87b6-99f0ca8db2d6"), Address = IPAddress.Parse("172.16.0.12"), Port=8443 }
            }
        },
        new TargetGroup {
            Id = Guid.Parse("2eedd7cc-fd16-4845-939a-457ea0b11c0a"),
            Name = "DnsGroup",
            Targets = new List<Target> {
                new Target { Id = Guid.Parse("266c7020-dde2-4a15-bd8f-0253cf7f2927"), Address = IPAddress.Parse("172.16.0.13"), Port=153 },
                new Target { Id = Guid.Parse("9273b8cd-58b4-44c6-8462-68bccafc4c70"), Address = IPAddress.Parse("172.16.0.14"), Port=153 },
                new Target { Id = Guid.Parse("9f871f53-6604-4dc0-b8d9-43187784556e"), Address = IPAddress.Parse("172.16.0.15"), Port=153 },
            }
        }
    };

    private static List<LoadBalancer> balancers = new List<LoadBalancer>
    {
        new LoadBalancer {
            Id = Guid.Parse("e8d5f029-4996-4bc1-86ec-5a92f0194b49"),
            Name = "FirstLoadBalancer",
            Enabled = true,
            Type = LoadBalancerType.NETWORK,
            Port = 6507,
            Group = targetGroups[0],
        },
        new LoadBalancer {
            Id = Guid.Parse("a550a4bd-2330-45b6-800f-345c1ffb5ff7"),
            Name = "SecondLoadBalancer",
            Enabled = true,
            Type = LoadBalancerType.APPLICATION,
            Port = 6508,
            Group = targetGroups[1],
        },
        new LoadBalancer {
            Id = Guid.Parse("a98a013d-31de-4a43-b990-c49658d3ed38"),
            Name = "ThirdLoadBalancer",
            Enabled = false,
            Type = LoadBalancerType.NETWORK,
            Port = 6509,
            Group = targetGroups[2]
        }
    };

    public LoadBalancerService(ILoadBalancingConfigurator configurator)
    {
        _configurator = configurator;
    }

    public List<LoadBalancer> GetLoadBalancers()
    {
        return balancers;
    }

    public LoadBalancer GetLoadBalancer(Guid Id)
    {
        return balancers.First(b => b.Id == Id);
    }

    public void AddLoadBalancer(LoadBalancer balancer)
    {
        balancers.Add(balancer);
    }

    public void UpdateLoadBalancer(LoadBalancer balancer)
    {
        var index = balancers.FindIndex(x => x.Id == balancer.Id);
        balancers[index] = balancer;
    }

    public void DeleteLoadBalancer(Guid Id)
    {
        balancers.RemoveAll(x => x.Id == Id);
    }

    public List<TargetGroup> GetTargetGroups()
    {
        return targetGroups;
    }

    public TargetGroup GetTargetGroup(Guid Id)
    {
        return targetGroups.First(t => t.Id == Id);
    }

    public void ApplyConfiguration()
    {
        var enabledBalancers = balancers.Where(b => b.Enabled == true).ToList();
        _configurator.ApplyConfiguration(enabledBalancers);
    }
}
