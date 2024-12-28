using System.Net;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System.Linq;


using Tilework.LoadBalancing.Persistence;
using Tilework.LoadBalancing.Persistence.Models;
using Tilework.LoadBalancing.Enums;
using Tilework.LoadBalancing.Interfaces;
using Tilework.LoadBalancing.Settings;

using Tilework.LoadBalancing.Haproxy;
using Microsoft.Extensions.DependencyInjection;

namespace Tilework.LoadBalancing.Services;


public class LoadBalancerService
{
    private readonly LoadBalancerContext _dbContext;
    private readonly LoadBalancerSettings _settings;
    private readonly ILoadBalancingConfigurator _configurator;
    private readonly ILogger<LoadBalancerService> _logger;
    
    
    // private static List<TargetGroup> targetGroups = new List<TargetGroup>
    // {
    //     new TargetGroup {
    //         Id = Guid.Parse("ec45a3e1-b807-4a48-8e27-8f60febb2c39"),
    //         Name = "ProxyGroup",
    //         Targets = new List<Target> {
    //             new Target { Id = Guid.Parse("0209dd00-732b-415a-81cf-13b5d9565938"), Address = IPAddress.Parse("172.16.0.10"), Port=8090 },
    //             new Target { Id = Guid.Parse("53ca44ae-b44e-4fce-8b75-af0b6121faf2"), Address = IPAddress.Parse("172.16.0.11"), Port=8090 }
    //         }
    //     },
    //     new TargetGroup {
    //         Id = Guid.Parse("1fa12cdc-72af-44ec-a780-e02157c30b01"),
    //         Name = "HttpGroup",
    //         Targets = new List<Target> {
    //             new Target { Id = Guid.Parse("01eb8280-f554-480e-87b6-99f0ca8db2d6"), Address = IPAddress.Parse("172.16.0.16"), Port=10080 }
    //         }
    //     },
    //     new TargetGroup {
    //         Id = Guid.Parse("2eedd7cc-fd16-4845-939a-457ea0b11c0a"),
    //         Name = "DnsGroup",
    //         Targets = new List<Target> {
    //             new Target { Id = Guid.Parse("266c7020-dde2-4a15-bd8f-0253cf7f2927"), Address = IPAddress.Parse("172.16.0.13"), Port=153 },
    //             new Target { Id = Guid.Parse("9273b8cd-58b4-44c6-8462-68bccafc4c70"), Address = IPAddress.Parse("172.16.0.14"), Port=153 },
    //             new Target { Id = Guid.Parse("9f871f53-6604-4dc0-b8d9-43187784556e"), Address = IPAddress.Parse("172.16.0.15"), Port=153 },
    //         }
    //     }
    // };

    // private static List<Listener> listeners = new List<Listener>
    // {
    //     new Listener {
    //         Id = Guid.Parse("522d8f9f-d066-47a0-826e-7ef5fb41c43a"),
    //         Port = 6507,
    //         Rules = new List<Rule> {
    //             new Rule {
    //                 Id = Guid.Parse("750438bd-3d97-4278-9c6e-a42f37c62bfa"),
    //                 Hostname = "proxy.tilework.dev",
    //                 Group = targetGroups[0]
    //             }
    //         }
    //     },
    //     new Listener {
    //         Id = Guid.Parse("0106d9b5-4787-45dc-9cf7-c49405ce96a0"),
    //         Port = 6508,
    //         Rules = new List<Rule> {
    //             new Rule {
    //                 Id = Guid.Parse("c773d315-6659-451b-af79-484428a501d2"),
    //                 Hostname = "www.tilework.dev",
    //                 Group = targetGroups[0]
    //             }
    //         }
    //     },
    //     new Listener {
    //         Id = Guid.Parse("03b79bda-37e9-4677-8d4d-0ea8323dec77"),
    //         Port = 6509,
    //         Rules = new List<Rule> {
    //             new Rule {
    //                 Id = Guid.Parse("87d15f5b-3049-4e09-9e04-7e02f74faaab"),
    //                 Hostname = "dns.tilework.dev",
    //                 Group = targetGroups[0]
    //             }
    //         }
    //     }
    // };

    // private static List<LoadBalancer> balancers = new List<LoadBalancer>
    // {
    //     new LoadBalancer {
    //         Id = Guid.Parse("e8d5f029-4996-4bc1-86ec-5a92f0194b49"),
    //         Name = "FirstLoadBalancer",
    //         Enabled = true,
    //         Type = LoadBalancerType.NETWORK,
    //         Listeners = new List<Listener> {
    //             listeners[0]
    //         }
    //     },
    //     new LoadBalancer {
    //         Id = Guid.Parse("a550a4bd-2330-45b6-800f-345c1ffb5ff7"),
    //         Name = "SecondLoadBalancer",
    //         Enabled = true,
    //         Type = LoadBalancerType.APPLICATION,
    //         Listeners = new List<Listener> {
    //             listeners[1]
    //         }
    //     },
    //     new LoadBalancer {
    //         Id = Guid.Parse("a98a013d-31de-4a43-b990-c49658d3ed38"),
    //         Name = "ThirdLoadBalancer",
    //         Enabled = false,
    //         Type = LoadBalancerType.NETWORK,
    //         Listeners = new List<Listener> {
    //             listeners[2]
    //         }
    //     }
    // };

    public LoadBalancerService(IServiceProvider serviceProvider,
                               LoadBalancerContext dbContext,
                               IOptions<LoadBalancerSettings> settings,
                               ILogger<LoadBalancerService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
        _settings = settings.Value;
        _configurator = LoadConfigurator(serviceProvider, _settings);
        
        ApplyConfiguration();
    }

    private ILoadBalancingConfigurator LoadConfigurator(IServiceProvider serviceProvider, LoadBalancerSettings settings)
    {
        return settings.Backend switch
        {
            "haproxy" => serviceProvider.GetRequiredService<HAProxyConfigurator>(),
            _ => throw new ArgumentException($"Invalid configurator in load balancing tile: {_settings.Backend}")
        };
    }

    public async Task<List<LoadBalancer>> GetLoadBalancers()
    {
        return await _dbContext.LoadBalancers.ToListAsync();
    }

    public async Task<LoadBalancer?> GetLoadBalancer(Guid Id)
    {
        return await _dbContext.LoadBalancers.FindAsync(Id);
    }

    public async Task AddLoadBalancer(LoadBalancer balancer)
    {
        await _dbContext.LoadBalancers.AddAsync(balancer);
        await _dbContext.SaveChangesAsync();
    }

    public async Task UpdateLoadBalancer(LoadBalancer balancer)
    {
        _dbContext.LoadBalancers.Update(balancer);
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteLoadBalancer(LoadBalancer balancer)
    {
        _dbContext.LoadBalancers.Remove(balancer);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<List<TargetGroup>> GetTargetGroups()
    {
        return await _dbContext.TargetGroups.ToListAsync();
    }

    public async Task<TargetGroup?> GetTargetGroup(Guid Id)
    {
        return await _dbContext.TargetGroups.FindAsync(Id);
    }

    public async Task AddTargetGroup(TargetGroup group)
    {
        await _dbContext.TargetGroups.AddAsync(group);
        await _dbContext.SaveChangesAsync();
    }

    public async Task UpdateTargetGroup(TargetGroup group)
    {
        _dbContext.TargetGroups.Update(group);
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteTargetGroup(TargetGroup group)
    {
        _dbContext.TargetGroups.Remove(group);
        await _dbContext.SaveChangesAsync();
    }

    // public async Task AddTarget(Target target)
    // {
    //     await _dbContext.Targets.AddAsync(target);
    //     await _dbContext.SaveChangesAsync();
    // }

    // public async Task DeleteTarget(Target target)
    // {
    //     _dbContext.Targets.Remove(target);
    //     await _dbContext.SaveChangesAsync();
    // }

    public async Task ApplyConfiguration()
    {
        var balancers = await _dbContext.LoadBalancers.ToListAsync();
        await _configurator.ApplyConfiguration(balancers);
    }
}
