using Tilework.LoadBalancing.Enums;

namespace Tilework.LoadBalancing.Persistence.Models;


public class LoadBalancer
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public LoadBalancerType Type { get; set; }
    public virtual List<Listener> Listeners { get; set; } = new List<Listener>();
    public bool Enabled { get; set; }
}