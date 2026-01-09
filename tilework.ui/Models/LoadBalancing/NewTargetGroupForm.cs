using System.ComponentModel.DataAnnotations;
using Tilework.LoadBalancing.Enums;

namespace Tilework.Ui.Models;

public class NewTargetGroupForm : BaseForm
{
    [Required]
    public string Name { get; set; }
    public TargetGroupProtocol Protocol { get; set; } 
}