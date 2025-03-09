using Microsoft.EntityFrameworkCore;

using Tilework.CertificateManagement.Persistence.Models;

namespace Tilework.CertificateManagement.Persistence;

public class CertificateManagementContext : DbContext
{
    public CertificateManagementContext(DbContextOptions<CertificateManagementContext> options) : base(options) { }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseLazyLoadingProxies();
    }

    public DbSet<Certificate> Certificates { get; set; }
    public DbSet<PrivateKey> PrivateKeys { get; set; }
    public DbSet<CertificateAuthority> CertificateAuthorities { get; set; }
}
