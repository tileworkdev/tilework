using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

using Tilework.LoadBalancing.Enums;
using Tilework.Persistence.CertificateManagement.Models;

namespace Tilework.Persistence.LoadBalancing.Models;

[Index(nameof(Name), IsUnique = true)]
public class LoadBalancer
{
    public Guid Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public int Port { get; set; }
    public LoadBalancerType Type { get; set; }
    public LoadBalancerProtocol Protocol { get; set; }

    public bool Enabled { get; set; }

    public virtual List<Certificate> Certificates { get; set; } = new();
    
    public virtual List<Rule> Rules { get; set; } = new();
}
