namespace Tilework.LoadBalancing.Models;

public class TargetGroup
{
    public string Name { get; set; }
    public List<Target> Targets { get; set; }
}