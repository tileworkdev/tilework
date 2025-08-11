using System.ComponentModel.DataAnnotations;
using Tilework.LoadBalancing.Enums;

namespace Tilework.LoadBalancing.Persistence.Models;

public class ApplicationLoadBalancer : BaseLoadBalancer
{
    public AlbProtocol Protocol { get; set; }

    public virtual List<Rule> Rules { get; set; }
}