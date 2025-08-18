using System.ComponentModel.DataAnnotations;
using Tilework.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Tilework.Persistence.LoadBalancing.Models;


[Index(nameof(TargetGroupId), nameof(Host), nameof(Port), IsUnique = true)]
public class Target
{
    public Guid Id { get; set; }

    [Required]
    public Host Host { get; set; }
    public int Port { get; set; }

    public Guid TargetGroupId { get; set; }
    public virtual TargetGroup TargetGroup { get; set; }
}