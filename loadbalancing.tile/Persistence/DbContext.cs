using Microsoft.EntityFrameworkCore;

using Tilework.LoadBalancing.Persistence.Models;

namespace Tilework.LoadBalancing.Persistence;

public class LoadBalancerContext : DbContext
{
    public LoadBalancerContext(DbContextOptions<LoadBalancerContext> options) : base(options) { }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseLazyLoadingProxies();
    }

    public DbSet<LoadBalancer> LoadBalancers { get; set; }
    public DbSet<Listener> Listeners { get; set; }
    public DbSet<Rule> Rules { get; set; }
    public DbSet<TargetGroup> TargetGroups { get; set; }
    public DbSet<Target> Targets { get; set; }
}
