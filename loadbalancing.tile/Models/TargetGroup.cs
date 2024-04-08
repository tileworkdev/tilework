namespace Tilework.LoadBalancing.Models;

public class TargetGroup
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public List<Target> Targets { get; set; }
}