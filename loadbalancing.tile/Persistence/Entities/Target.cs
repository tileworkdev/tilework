using System.ComponentModel.DataAnnotations;
using Tilework.Core.Models;

namespace Tilework.LoadBalancing.Persistence.Models;


public class Target
{
    public Guid Id { get; set; }

    [Required]
    public Host Host { get; set; }
    public int Port { get; set; }

    public Guid TargetGroupId { get; set; }
    public virtual TargetGroup TargetGroup { get; set; }
}