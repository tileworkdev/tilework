using Tilework.Core.Models;

namespace Tilework.LoadBalancing.Models;


public class TargetDTO
{
    public Guid Id { get; set; }
    public Host Host { get; set; }
    public int Port { get; set; }
}