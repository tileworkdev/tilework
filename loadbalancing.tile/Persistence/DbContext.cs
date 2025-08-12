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


    public DbSet<BaseLoadBalancer> LoadBalancers { get; set; }
    public DbSet<ApplicationLoadBalancer> ApplicationLoadBalancers { get; set; }
    public DbSet<NetworkLoadBalancer> NetworkLoadBalancers { get; set; }
    public DbSet<Rule> Rules { get; set; }
    public DbSet<TargetGroup> TargetGroups { get; set; }
    public DbSet<Target> Targets { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Rule>()
        .OwnsMany(r => r.Conditions, b =>
        {
            b.ToJson();
        });
    }
}
