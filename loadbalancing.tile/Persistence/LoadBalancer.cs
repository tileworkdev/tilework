using System.ComponentModel.DataAnnotations;
using Tilework.LoadBalancing.Enums;

namespace Tilework.LoadBalancing.Persistence.Models;


public class LoadBalancer
{
    public Guid Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty; 
    public LoadBalancerType Type { get; set; }
    public virtual List<NetworkListener> NetworkListeners { get; set; } = new List<NetworkListener>();
    public virtual List<ApplicationListener> ApplicationListeners { get; set; } = new List<ApplicationListener>();
    public bool Enabled { get; set; }
}