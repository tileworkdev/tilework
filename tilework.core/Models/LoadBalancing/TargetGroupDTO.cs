using Tilework.LoadBalancing.Enums;

namespace Tilework.LoadBalancing.Models;


public class TargetGroupDTO
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public TargetGroupProtocol Protocol { get; set; }
}