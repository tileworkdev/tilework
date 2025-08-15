using Tilework.Core.LoadBalancing.Enums;

namespace Tilework.LoadBalancing.Persistence.Models;

public class ApplicationLoadBalancer : BaseLoadBalancer
{
    public AlbProtocol Protocol { get; set; }

    public virtual List<Rule> Rules { get; set; } = new();
}