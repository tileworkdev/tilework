using Tilework.Core.LoadBalancing.Enums;

namespace Tilework.Core.LoadBalancing.Models;


public class TargetGroupDTO
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public TargetGroupProtocol Protocol { get; set; }
}