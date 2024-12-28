using Tilework.LoadBalancing.Enums;

namespace Tilework.LoadBalancing.Persistence.Models;

public class Listener
{
    public Guid Id { get; set; }
    public int Port { get; set; }

    public AlbProtocol? AlbProtocol { get; set; }

    public NlbProtocol? NlbProtocol { get; set; }

    public virtual List<Rule> Rules { get; set; }
    public bool Enabled { get; set; }

    public Guid LoadBalancerId { get; set; }
    public virtual LoadBalancer LoadBalancer { get; set; }
}