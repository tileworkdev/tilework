using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Text.Json;

using Tilework.Core.Models;

using Tilework.Persistence.LoadBalancing.Models;
using Tilework.Persistence.CertificateManagement.Models;

namespace Tilework.Core.Persistence;

public class TileworkContext : DbContext
{
    public TileworkContext(DbContextOptions<TileworkContext> options) : base(options) { }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseLazyLoadingProxies();
    }

    // Load balancing
    public DbSet<BaseLoadBalancer> LoadBalancers { get; set; }
    public DbSet<ApplicationLoadBalancer> ApplicationLoadBalancers { get; set; }
    public DbSet<NetworkLoadBalancer> NetworkLoadBalancers { get; set; }
    public DbSet<Rule> Rules { get; set; }
    public DbSet<TargetGroup> TargetGroups { get; set; }
    public DbSet<Target> Targets { get; set; }


    // Certificate management
    public DbSet<Certificate> Certificates { get; set; }
    public DbSet<PrivateKey> PrivateKeys { get; set; }
    public DbSet<CertificateAuthority> CertificateAuthorities { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        // Load balancing
        modelBuilder.Entity<Rule>()
        .OwnsMany(r => r.Conditions, b =>
        {
            b.ToJson();
        });

        modelBuilder.Entity<Target>()
            .Property(e => e.Host)
            .HasConversion(
                v => v.Value,
                v => Host.Parse(v))
            .HasMaxLength(253);

        modelBuilder.Entity<Target>()
            .Property(e => e.Host)
            .Metadata.SetValueComparer(new ValueComparer<Host>(
                (a, b) => a.Value == b.Value,
                v => v.Value.GetHashCode(),
                v => new Host(v.Value)));


        // Certificate management
        modelBuilder.Entity<Certificate>()
            .Property(x => x.ExpiresAtUtc)
            .HasConversion(
                v => v.HasValue ? v.Value.ToUnixTimeSeconds() : (long?)null,
                v => v.HasValue ? DateTimeOffset.FromUnixTimeSeconds(v.Value) : null
            )
            .HasColumnType("INTEGER");
    }
}