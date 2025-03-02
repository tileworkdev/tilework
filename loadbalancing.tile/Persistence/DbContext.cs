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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LoadBalancer>()
            .HasMany(lb => lb.NetworkListeners)
            .WithOne(l => l.LoadBalancer)
            .HasForeignKey(l => l.LoadBalancerId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<LoadBalancer>()
            .HasMany(lb => lb.ApplicationListeners)
            .WithOne(l => l.LoadBalancer)
            .HasForeignKey(l => l.LoadBalancerId)
            .OnDelete(DeleteBehavior.Cascade);

        base.OnModelCreating(modelBuilder);
    }

    public DbSet<LoadBalancer> LoadBalancers { get; set; }
    public DbSet<ApplicationListener> ApplicationListeners { get; set; }
    public DbSet<NetworkListener> NetworkListeners { get; set; }
    public DbSet<Rule> Rules { get; set; }
    public DbSet<TargetGroup> TargetGroups { get; set; }
    public DbSet<Target> Targets { get; set; }
}
