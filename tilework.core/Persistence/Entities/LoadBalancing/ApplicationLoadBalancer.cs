using Tilework.LoadBalancing.Enums;

namespace Tilework.Persistence.LoadBalancing.Models;

public class ApplicationLoadBalancer : BaseLoadBalancer
{
    public AlbProtocol Protocol { get; set; }

    public virtual List<Rule> Rules { get; set; } = new();
}