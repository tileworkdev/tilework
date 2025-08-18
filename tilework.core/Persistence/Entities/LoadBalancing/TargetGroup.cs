using Microsoft.EntityFrameworkCore;
using Tilework.LoadBalancing.Enums;

namespace Tilework.Persistence.LoadBalancing.Models;

[Index(nameof(Name), IsUnique = true)]
public class TargetGroup
{
    public Guid Id { get; set; }
    public string Name { get; set; }

    public TargetGroupProtocol Protocol { get; set; }

    public virtual List<Target> Targets { get; set; } = new List<Target>();
}