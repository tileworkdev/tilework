namespace Tilework.LoadBalancing.Models;


public class BaseLoadBalancerDTO
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public int Port { get; set; }

    public bool Enabled { get; set; }
}