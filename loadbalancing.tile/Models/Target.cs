using System.Net;


namespace Tilework.LoadBalancing.Models;

public class Target
{
    public Guid Id { get; set; }
    public IPAddress Address { get; set; }
}