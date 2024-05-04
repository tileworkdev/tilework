using Tilework.LoadBalancing.Enums;

namespace Tilework.LoadBalancing.Models;

public class LoadBalancer
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public LoadBalancerType Type { get; set; }
    public int Port { get; set; }
    public TargetGroup Group { get; set; }
    public bool Enabled { get; set; }
}