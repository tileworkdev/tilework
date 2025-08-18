using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Tilework.Persistence.LoadBalancing.Models;

[Index(nameof(Name), IsUnique = true)]
public abstract class BaseLoadBalancer
{
    public Guid Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public int Port { get; set; }

    public bool Enabled { get; set; }

    public List<Guid> CertificateIds { get; set; } = new();
}