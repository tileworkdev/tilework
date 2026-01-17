using Tilework.LoadBalancing.Enums;

namespace Tilework.LoadBalancing.Models;


public class LoadBalancerDTO
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public int Port { get; set; }

    public bool Enabled { get; set; }
    public LoadBalancerType Type { get; set; }
    public LoadBalancerProtocol Protocol { get; set; }
}