using Tilework.LoadBalancing.Enums;

namespace Tilework.LoadBalancing.Persistence.Models;

public class ApplicationListener
{
    public Guid Id { get; set; }
    public int Port { get; set; }

    public AlbProtocol Protocol { get; set; }

    public virtual List<Rule> Rules { get; set; }

    public Guid LoadBalancerId { get; set; }
    public virtual LoadBalancer LoadBalancer { get; set; }
}