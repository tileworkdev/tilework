namespace Tilework.LoadBalancing.Persistence.Models;

public class TargetGroup
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public virtual List<Target> Targets { get; set; } = new List<Target>();
}