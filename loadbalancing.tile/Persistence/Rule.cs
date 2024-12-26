using Tilework.LoadBalancing.Enums;

namespace Tilework.LoadBalancing.Persistence.Models;


public class Rule
{
    public Guid Id { get; set; }
    public string Hostname { get; set; }
    public virtual TargetGroup Group { get; set; }

    public Guid ListenerId { get; set; }
    public virtual Listener Listener { get; set; }
}